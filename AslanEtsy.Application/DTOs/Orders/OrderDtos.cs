using AslanEtsy.Domain.Enums;

namespace AslanEtsy.Application.DTOs.Orders;

public class OrderDto
{
    public int Id { get; set; }
    public int EtsyAccountId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public long ReceiptId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    
    public decimal GrandTotalAmount { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    
    public bool IsPaid { get; set; }
    public bool IsShipped { get; set; }
    public OrderStatus Status { get; set; }
    public CustomOrderStatus CustomStatus { get; set; }
    
    public DateTime OrderDateUtc { get; set; }
    public DateTime? PaidDateUtc { get; set; }
    public DateTime? ShippedDateUtc { get; set; }
    public DateTime? ExpectedShipDateUtc { get; set; }
    
    public string? ShippingCity { get; set; }
    public string? ShippingCountry { get; set; }
    public string? ShippingCountryIso { get; set; }
    
    public int ItemCount { get; set; }
    public string? InternalNote { get; set; }
    public string? Tags { get; set; }
    public bool HasTracking { get; set; }
    public string? LatestTrackingCode { get; set; }
    public string? LatestCarrierName { get; set; }
    public DateTime? LatestShipDateUtc { get; set; }
    public bool IsLatestTrackingSynced { get; set; }
}

public class OrderDetailDto : OrderDto
{
    public long? BuyerUserId { get; set; }
    public string? RecipientName { get; set; }
    public string? ShippingFirstLine { get; set; }
    public string? ShippingSecondLine { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingZip { get; set; }
    public string? ShippingAddressFormatted { get; set; }
    
    public string? MessageFromBuyer { get; set; }
    public string? MessageFromPayment { get; set; }
    public bool IsGift { get; set; }
    public string? GiftMessage { get; set; }
    
    public List<OrderItemDto> Items { get; set; } = new();
    public List<OrderTrackingDto> Trackings { get; set; } = new();
}

public class OrderItemDto
{
    public int Id { get; set; }
    public long TransactionId { get; set; }
    public long ListingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
    public string CurrencyCode { get; set; } = "USD";
    public string? Sku { get; set; }
    public string? ImageUrl { get; set; }
    public string? VariationsSummary { get; set; }
    public bool IsCustomOrder { get; set; }
    public string? BuyerPersonalization { get; set; }
}

public class OrderTrackingDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string TrackingCode { get; set; } = string.Empty;
    public string CarrierName { get; set; } = string.Empty;
    public DateTime ShipDateUtc { get; set; }
    public string? TrackingUrl { get; set; }
    public bool IsSyncedToEtsy { get; set; }
    public DateTime? SyncedToEtsyAtUtc { get; set; }
    public string? EtsySyncErrorMessage { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class CreateOrderTrackingDto
{
    public string TrackingCode { get; set; } = string.Empty;
    public string CarrierName { get; set; } = string.Empty;
    public DateTime? ShipDateUtc { get; set; }
    public string? Note { get; set; }
    public bool SendToEtsyImmediately { get; set; } = true;
}

public class UpdateOrderDto
{
    public CustomOrderStatus? CustomStatus { get; set; }
    public string? InternalNote { get; set; }
    public string? Tags { get; set; }
}

public class OrderFilterRequest
{
    public int? EtsyAccountId { get; set; }
    public OrderStatus? Status { get; set; }
    public CustomOrderStatus? CustomStatus { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
