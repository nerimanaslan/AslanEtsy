using System.Text.Json;
using AslanEtsy.Application.DTOs.Sync;
using AslanEtsy.Application.Interfaces;
using AslanEtsy.Domain.Entities;
using AslanEtsy.Domain.Enums;
using AslanEtsy.Domain.Interfaces;

namespace AslanEtsy.Application.Services;

public class EtsySyncService : IEtsySyncService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEtsyApiClient _etsyApiClient;
    private readonly IEtsyAccountService _accountService;

    public EtsySyncService(
        IUnitOfWork unitOfWork,
        IEtsyApiClient etsyApiClient,
        IEtsyAccountService accountService)
    {
        _unitOfWork = unitOfWork;
        _etsyApiClient = etsyApiClient;
        _accountService = accountService;
    }

    public async Task<SyncResultDto> SyncAccountOrdersAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(accountId, cancellationToken);
        if (account == null)
            throw new KeyNotFoundException($"Mağaza bulunamadı (ID: {accountId})");

        var syncLog = new SyncLog
        {
            EtsyAccountId = account.Id,
            StartedAtUtc = DateTime.UtcNow,
            Status = SyncStatus.InProgress
        };

        await _unitOfWork.SyncLogs.AddAsync(syncLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = new SyncResultDto
        {
            EtsyAccountId = account.Id,
            ShopName = account.ShopName,
            Status = SyncStatus.InProgress
        };

        if (string.IsNullOrWhiteSpace(account.AccessToken))
        {
            var msg = "Mağazanın Etsy bağlantısı (Access Token) bulunamadı. Lütfen önce mağaza hesabını bağlayın.";
            syncLog.Status = SyncStatus.Failed;
            syncLog.ErrorMessage = msg;
            syncLog.CompletedAtUtc = DateTime.UtcNow;
            _unitOfWork.SyncLogs.Update(syncLog);
            account.LastSyncError = msg;
            _unitOfWork.Accounts.Update(account);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            result.Status = SyncStatus.Failed;
            result.ErrorMessage = msg;
            return result;
        }

        // Refresh token if needed
        await _accountService.RefreshAccountTokenIfNeededAsync(account.Id, cancellationToken);

        try
        {
            // Fetch receipts from Etsy API v3 (up to 100 recent orders)
            var response = await _etsyApiClient.GetShopReceiptsAsync(
                account.Keystring,
                account.AccessToken,
                account.ShopId,
                limit: 100,
                cancellationToken: cancellationToken);

            if (response == null || response.results == null)
            {
                throw new Exception("Etsy API'den sipariş listesi alınamadı.");
            }

            int createdCount = 0;
            int updatedCount = 0;

            foreach (var r in response.results)
            {
                var existingOrder = await _unitOfWork.Orders.GetByReceiptIdAsync(r.receipt_id, cancellationToken);

                if (existingOrder == null)
                {
                    // Create new order
                    var order = MapReceiptToNewOrder(r, account.Id);
                    await _unitOfWork.Orders.AddAsync(order, cancellationToken);
                    createdCount++;
                }
                else
                {
                    // Update existing order status/dates while preserving user notes/tags
                    UpdateExistingOrder(existingOrder, r);
                    _unitOfWork.Orders.Update(existingOrder);
                    updatedCount++;
                }
            }

            account.LastSyncAtUtc = DateTime.UtcNow;
            account.LastSyncError = null;
            _unitOfWork.Accounts.Update(account);

            syncLog.Status = SyncStatus.Success;
            syncLog.OrdersFetched = response.results.Count;
            syncLog.OrdersCreated = createdCount;
            syncLog.OrdersUpdated = updatedCount;
            syncLog.CompletedAtUtc = DateTime.UtcNow;
            _unitOfWork.SyncLogs.Update(syncLog);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            result.Status = SyncStatus.Success;
            result.OrdersFetched = response.results.Count;
            result.OrdersCreated = createdCount;
            result.OrdersUpdated = updatedCount;
            return result;
        }
        catch (Exception ex)
        {
            syncLog.Status = SyncStatus.Failed;
            syncLog.ErrorMessage = ex.Message;
            syncLog.CompletedAtUtc = DateTime.UtcNow;
            _unitOfWork.SyncLogs.Update(syncLog);

            account.LastSyncError = ex.Message;
            _unitOfWork.Accounts.Update(account);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            result.Status = SyncStatus.Failed;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    public async Task<List<SyncResultDto>> SyncAllActiveAccountsAsync(CancellationToken cancellationToken = default)
    {
        var accounts = await _unitOfWork.Accounts.GetActiveAccountsAsync(cancellationToken);
        var results = new List<SyncResultDto>();

        foreach (var account in accounts.Where(a => a.AutoSyncEnabled && !string.IsNullOrWhiteSpace(a.AccessToken)))
        {
            try
            {
                var res = await SyncAccountOrdersAsync(account.Id, cancellationToken);
                results.Add(res);
            }
            catch (Exception ex)
            {
                results.Add(new SyncResultDto
                {
                    EtsyAccountId = account.Id,
                    ShopName = account.ShopName,
                    Status = SyncStatus.Failed,
                    ErrorMessage = ex.Message
                });
            }
        }

        return results;
    }

    public async Task<IReadOnlyList<SyncLogDto>> GetSyncLogsAsync(int? accountId = null, int limit = 50, CancellationToken cancellationToken = default)
    {
        var logs = await _unitOfWork.SyncLogs.FindAsync(
            l => (!accountId.HasValue || l.EtsyAccountId == accountId.Value) && !l.IsDeleted,
            cancellationToken);

        return logs
            .OrderByDescending(l => l.StartedAtUtc)
            .Take(limit)
            .Select(l => new SyncLogDto
            {
                Id = l.Id,
                EtsyAccountId = l.EtsyAccountId,
                ShopName = l.EtsyAccount?.ShopName ?? "Mağaza #" + l.EtsyAccountId,
                StartedAtUtc = l.StartedAtUtc,
                CompletedAtUtc = l.CompletedAtUtc,
                Status = l.Status,
                OrdersFetched = l.OrdersFetched,
                OrdersCreated = l.OrdersCreated,
                OrdersUpdated = l.OrdersUpdated,
                ErrorMessage = l.ErrorMessage
            })
            .ToList();
    }

    private static Order MapReceiptToNewOrder(EtsyReceiptResponse r, int accountId)
    {
        var order = new Order
        {
            EtsyAccountId = accountId,
            ReceiptId = r.receipt_id,
            BuyerUserId = r.buyer_user_id > 0 ? r.buyer_user_id : null,
            BuyerName = !string.IsNullOrWhiteSpace(r.name) ? r.name : "İsimsiz Müşteri",
            BuyerEmail = r.buyer_email ?? string.Empty,
            GrandTotalAmount = r.grandtotal?.DecimalValue ?? 0,
            SubtotalAmount = r.subtotal?.DecimalValue ?? 0,
            ShippingAmount = r.total_shipping_cost?.DecimalValue ?? 0,
            TaxAmount = r.total_tax_cost?.DecimalValue ?? 0,
            DiscountAmount = r.discount_amt?.DecimalValue ?? 0,
            CurrencyCode = r.grandtotal?.currency_code ?? "USD",
            IsPaid = r.is_paid,
            IsShipped = r.is_shipped,
            Status = r.is_shipped ? OrderStatus.Shipped : (r.is_paid ? OrderStatus.Paid : OrderStatus.Open),
            CustomStatus = r.is_shipped ? CustomOrderStatus.Shipped : CustomOrderStatus.New,
            OrderDateUtc = UnixTimeStampToDateTime(r.create_timestamp > 0 ? r.create_timestamp : r.created_timestamp),
            PaidDateUtc = r.is_paid ? DateTime.UtcNow : null,
            ShippedDateUtc = r.is_shipped ? DateTime.UtcNow : null,
            RecipientName = r.name,
            ShippingFirstLine = r.first_line,
            ShippingSecondLine = r.second_line,
            ShippingCity = r.city,
            ShippingState = r.state,
            ShippingZip = r.zip,
            ShippingCountryIso = r.country_iso,
            ShippingAddressFormatted = r.formatted_address,
            MessageFromBuyer = r.message_from_buyer,
            MessageFromPayment = r.message_from_payment,
            IsGift = r.is_gift,
            GiftMessage = r.gift_message,
            RawJson = JsonSerializer.Serialize(r)
        };

        // Add line items
        if (r.transactions != null)
        {
            foreach (var t in r.transactions)
            {
                var variationsSummary = FormatVariations(t.variations);
                order.Items.Add(new OrderItem
                {
                    TransactionId = t.transaction_id,
                    ListingId = t.listing_id,
                    Title = t.title ?? "Ürün #" + t.listing_id,
                    Description = t.description,
                    Quantity = t.quantity,
                    UnitPrice = t.price?.DecimalValue ?? 0,
                    CurrencyCode = t.price?.currency_code ?? order.CurrencyCode,
                    Sku = t.sku,
                    ImageUrl = t.image_url,
                    VariationsSummary = variationsSummary,
                    BuyerPersonalization = t.buyer_personalization,
                    IsCustomOrder = !string.IsNullOrWhiteSpace(t.buyer_personalization)
                });
            }
        }

        // Add shipments if any
        if (r.shipments != null)
        {
            foreach (var s in r.shipments)
            {
                if (!string.IsNullOrWhiteSpace(s.tracking_code))
                {
                    order.Trackings.Add(new OrderTracking
                    {
                        TrackingCode = s.tracking_code,
                        CarrierName = s.carrier_name ?? "Diğer",
                        ShipDateUtc = s.ship_date > 0 ? UnixTimeStampToDateTime(s.ship_date) : DateTime.UtcNow,
                        IsSyncedToEtsy = true,
                        SyncedToEtsyAtUtc = DateTime.UtcNow
                    });
                }
            }
        }

        return order;
    }

    private static void UpdateExistingOrder(Order existing, EtsyReceiptResponse r)
    {
        existing.IsPaid = r.is_paid;
        existing.IsShipped = r.is_shipped;
        if (r.is_shipped)
        {
            existing.Status = OrderStatus.Shipped;
            if (existing.CustomStatus == CustomOrderStatus.New || existing.CustomStatus == CustomOrderStatus.ReadyToShip)
            {
                existing.CustomStatus = CustomOrderStatus.Shipped;
            }
        }

        existing.GrandTotalAmount = r.grandtotal?.DecimalValue ?? existing.GrandTotalAmount;
        existing.ShippingAddressFormatted = r.formatted_address ?? existing.ShippingAddressFormatted;
        existing.ShippingCity = r.city ?? existing.ShippingCity;
        existing.ShippingState = r.state ?? existing.ShippingState;
        existing.ShippingZip = r.zip ?? existing.ShippingZip;
        existing.ShippingCountryIso = r.country_iso ?? existing.ShippingCountryIso;
        existing.UpdatedAtUtc = DateTime.UtcNow;
    }

    private static string? FormatVariations(List<EtsyVariationValueResponse>? variations)
    {
        if (variations == null || variations.Count == 0) return null;
        return string.Join(", ", variations.Select(v => $"{v.formatted_name}: {v.formatted_value}"));
    }

    private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        if (unixTimeStamp <= 0) return DateTime.UtcNow;
        var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp);
        return dateTimeOffset.UtcDateTime;
    }
}
