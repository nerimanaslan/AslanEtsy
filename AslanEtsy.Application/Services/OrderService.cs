using AslanEtsy.Application.DTOs.Common;
using AslanEtsy.Application.DTOs.Orders;
using AslanEtsy.Application.Interfaces;
using AslanEtsy.Domain.Entities;
using AslanEtsy.Domain.Enums;
using AslanEtsy.Domain.Interfaces;

namespace AslanEtsy.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEtsyApiClient _etsyApiClient;
    private readonly IEtsyAccountService _accountService;

    public OrderService(
        IUnitOfWork unitOfWork,
        IEtsyApiClient etsyApiClient,
        IEtsyAccountService accountService)
    {
        _unitOfWork = unitOfWork;
        _etsyApiClient = etsyApiClient;
        _accountService = accountService;
    }

    public async Task<PagedResult<OrderDto>> GetOrdersAsync(OrderFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetOrdersFilteredAsync(
            filter.EtsyAccountId,
            filter.Status,
            filter.CustomStatus,
            filter.SearchTerm,
            filter.StartDate,
            filter.EndDate,
            filter.PageNumber,
            filter.PageSize,
            cancellationToken);

        var totalCount = await _unitOfWork.Orders.GetOrdersCountFilteredAsync(
            filter.EtsyAccountId,
            filter.Status,
            filter.CustomStatus,
            filter.SearchTerm,
            filter.StartDate,
            filter.EndDate,
            cancellationToken);

        return new PagedResult<OrderDto>
        {
            Items = orders.Select(MapToOrderDto).ToList(),
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<OrderDetailDto?> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(id, cancellationToken);
        return order != null ? MapToOrderDetailDto(order) : null;
    }

    public async Task<OrderDetailDto?> GetOrderByReceiptIdAsync(long receiptId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByReceiptIdAsync(receiptId, cancellationToken);
        return order != null ? MapToOrderDetailDto(order) : null;
    }

    public async Task<OrderDetailDto?> UpdateOrderDetailsAsync(int id, UpdateOrderDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(id, cancellationToken);
        if (order == null) return null;

        if (dto.CustomStatus.HasValue)
        {
            order.CustomStatus = dto.CustomStatus.Value;
        }

        if (dto.InternalNote != null)
        {
            order.InternalNote = dto.InternalNote;
        }

        if (dto.Tags != null)
        {
            order.Tags = dto.Tags;
        }

        order.UpdatedAtUtc = DateTime.UtcNow;
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToOrderDetailDto(order);
    }

    public async Task<OrderTrackingDto?> AddTrackingAsync(int orderId, CreateOrderTrackingDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(orderId, cancellationToken);
        if (order == null) return null;

        var tracking = new OrderTracking
        {
            OrderId = orderId,
            TrackingCode = dto.TrackingCode.Trim(),
            CarrierName = dto.CarrierName.Trim(),
            ShipDateUtc = dto.ShipDateUtc ?? DateTime.UtcNow,
            Note = dto.Note,
            IsSyncedToEtsy = false
        };

        await _unitOfWork.OrderTrackings.AddAsync(tracking, cancellationToken);

        // Update order status
        order.IsShipped = true;
        order.ShippedDateUtc = tracking.ShipDateUtc;
        order.Status = OrderStatus.Shipped;
        order.CustomStatus = CustomOrderStatus.Shipped;
        order.UpdatedAtUtc = DateTime.UtcNow;

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send to Etsy API if requested
        if (dto.SendToEtsyImmediately)
        {
            await PushTrackingToEtsyAsync(order, tracking, cancellationToken);
            _unitOfWork.OrderTrackings.Update(tracking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return MapToTrackingDto(tracking);
    }

    public async Task<bool> ResyncTrackingToEtsyAsync(int trackingId, CancellationToken cancellationToken = default)
    {
        var tracking = await _unitOfWork.OrderTrackings.GetByIdAsync(trackingId, cancellationToken);
        if (tracking == null) return false;

        var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(tracking.OrderId, cancellationToken);
        if (order == null) return false;

        var success = await PushTrackingToEtsyAsync(order, tracking, cancellationToken);
        _unitOfWork.OrderTrackings.Update(tracking);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return success;
    }

    public async Task<bool> DeleteTrackingAsync(int trackingId, CancellationToken cancellationToken = default)
    {
        var tracking = await _unitOfWork.OrderTrackings.GetByIdAsync(trackingId, cancellationToken);
        if (tracking == null) return false;

        tracking.IsDeleted = true;
        tracking.UpdatedAtUtc = DateTime.UtcNow;
        _unitOfWork.OrderTrackings.Update(tracking);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<bool> PushTrackingToEtsyAsync(Order order, OrderTracking tracking, CancellationToken cancellationToken)
    {
        var account = order.EtsyAccount ?? await _unitOfWork.Accounts.GetByIdAsync(order.EtsyAccountId, cancellationToken);
        if (account == null || string.IsNullOrWhiteSpace(account.AccessToken))
        {
            tracking.EtsySyncErrorMessage = "Mağaza bağlı değil veya access token eksik.";
            return false;
        }

        // Refresh token if expired
        await _accountService.RefreshAccountTokenIfNeededAsync(account.Id, cancellationToken);

        try
        {
            var success = await _etsyApiClient.CreateReceiptShipmentAsync(
                account.Keystring,
                account.AccessToken,
                account.ShopId,
                order.ReceiptId,
                tracking.TrackingCode,
                tracking.CarrierName,
                false,
                cancellationToken);

            if (success)
            {
                tracking.IsSyncedToEtsy = true;
                tracking.SyncedToEtsyAtUtc = DateTime.UtcNow;
                tracking.EtsySyncErrorMessage = null;
                return true;
            }
            else
            {
                tracking.IsSyncedToEtsy = false;
                tracking.EtsySyncErrorMessage = "Etsy API kargo bildirimini kabul etmedi.";
                return false;
            }
        }
        catch (Exception ex)
        {
            tracking.IsSyncedToEtsy = false;
            tracking.EtsySyncErrorMessage = $"Etsy API Hatası: {ex.Message}";
            return false;
        }
    }

    private static OrderDto MapToOrderDto(Order o)
    {
        var latestTracking = o.Trackings?.Where(t => !t.IsDeleted).OrderByDescending(t => t.CreatedAtUtc).FirstOrDefault();
        return new OrderDto
        {
            Id = o.Id,
            EtsyAccountId = o.EtsyAccountId,
            ShopName = o.EtsyAccount?.ShopName ?? "Bilinmeyen Mağaza",
            ReceiptId = o.ReceiptId,
            BuyerName = o.BuyerName,
            BuyerEmail = o.BuyerEmail,
            GrandTotalAmount = o.GrandTotalAmount,
            SubtotalAmount = o.SubtotalAmount,
            ShippingAmount = o.ShippingAmount,
            TaxAmount = o.TaxAmount,
            DiscountAmount = o.DiscountAmount,
            CurrencyCode = o.CurrencyCode,
            IsPaid = o.IsPaid,
            IsShipped = o.IsShipped,
            Status = o.Status,
            CustomStatus = o.CustomStatus,
            OrderDateUtc = o.OrderDateUtc,
            PaidDateUtc = o.PaidDateUtc,
            ShippedDateUtc = o.ShippedDateUtc,
            ExpectedShipDateUtc = o.ExpectedShipDateUtc,
            ShippingCity = o.ShippingCity,
            ShippingCountry = o.ShippingCountry,
            ShippingCountryIso = o.ShippingCountryIso,
            ItemCount = o.Items?.Count ?? 0,
            InternalNote = o.InternalNote,
            Tags = o.Tags,
            HasTracking = latestTracking != null,
            LatestTrackingCode = latestTracking?.TrackingCode,
            LatestCarrierName = latestTracking?.CarrierName,
            LatestShipDateUtc = latestTracking?.ShipDateUtc,
            IsLatestTrackingSynced = latestTracking?.IsSyncedToEtsy ?? false
        };
    }

    private static OrderDetailDto MapToOrderDetailDto(Order o)
    {
        var dto = new OrderDetailDto
        {
            Id = o.Id,
            EtsyAccountId = o.EtsyAccountId,
            ShopName = o.EtsyAccount?.ShopName ?? "Bilinmeyen Mağaza",
            ReceiptId = o.ReceiptId,
            BuyerUserId = o.BuyerUserId,
            BuyerName = o.BuyerName,
            BuyerEmail = o.BuyerEmail,
            GrandTotalAmount = o.GrandTotalAmount,
            SubtotalAmount = o.SubtotalAmount,
            ShippingAmount = o.ShippingAmount,
            TaxAmount = o.TaxAmount,
            DiscountAmount = o.DiscountAmount,
            CurrencyCode = o.CurrencyCode,
            IsPaid = o.IsPaid,
            IsShipped = o.IsShipped,
            Status = o.Status,
            CustomStatus = o.CustomStatus,
            OrderDateUtc = o.OrderDateUtc,
            PaidDateUtc = o.PaidDateUtc,
            ShippedDateUtc = o.ShippedDateUtc,
            ExpectedShipDateUtc = o.ExpectedShipDateUtc,
            RecipientName = o.RecipientName,
            ShippingFirstLine = o.ShippingFirstLine,
            ShippingSecondLine = o.ShippingSecondLine,
            ShippingCity = o.ShippingCity,
            ShippingState = o.ShippingState,
            ShippingZip = o.ShippingZip,
            ShippingCountry = o.ShippingCountry,
            ShippingCountryIso = o.ShippingCountryIso,
            ShippingAddressFormatted = o.ShippingAddressFormatted,
            MessageFromBuyer = o.MessageFromBuyer,
            MessageFromPayment = o.MessageFromPayment,
            IsGift = o.IsGift,
            GiftMessage = o.GiftMessage,
            InternalNote = o.InternalNote,
            Tags = o.Tags,
            ItemCount = o.Items?.Count ?? 0,
            Items = o.Items?.Where(i => !i.IsDeleted).Select(MapToItemDto).ToList() ?? new(),
            Trackings = o.Trackings?.Where(t => !t.IsDeleted).OrderByDescending(t => t.CreatedAtUtc).Select(MapToTrackingDto).ToList() ?? new()
        };

        var latestTracking = dto.Trackings.FirstOrDefault();
        dto.HasTracking = latestTracking != null;
        dto.LatestTrackingCode = latestTracking?.TrackingCode;
        dto.LatestCarrierName = latestTracking?.CarrierName;
        dto.LatestShipDateUtc = latestTracking?.ShipDateUtc;
        dto.IsLatestTrackingSynced = latestTracking?.IsSyncedToEtsy ?? false;

        return dto;
    }

    private static OrderItemDto MapToItemDto(OrderItem i)
    {
        return new OrderItemDto
        {
            Id = i.Id,
            TransactionId = i.TransactionId,
            ListingId = i.ListingId,
            Title = i.Title,
            Description = i.Description,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            CurrencyCode = i.CurrencyCode,
            Sku = i.Sku,
            ImageUrl = i.ImageUrl,
            VariationsSummary = i.VariationsSummary,
            IsCustomOrder = i.IsCustomOrder,
            BuyerPersonalization = i.BuyerPersonalization
        };
    }

    private static OrderTrackingDto MapToTrackingDto(OrderTracking t)
    {
        return new OrderTrackingDto
        {
            Id = t.Id,
            OrderId = t.OrderId,
            TrackingCode = t.TrackingCode,
            CarrierName = t.CarrierName,
            ShipDateUtc = t.ShipDateUtc,
            TrackingUrl = t.TrackingUrl,
            IsSyncedToEtsy = t.IsSyncedToEtsy,
            SyncedToEtsyAtUtc = t.SyncedToEtsyAtUtc,
            EtsySyncErrorMessage = t.EtsySyncErrorMessage,
            Note = t.Note,
            CreatedAtUtc = t.CreatedAtUtc
        };
    }
}
