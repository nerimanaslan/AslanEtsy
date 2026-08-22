using AslanEtsy.Domain.Common;
using AslanEtsy.Domain.Enums;

namespace AslanEtsy.Domain.Entities;

public class Order : BaseEntity
{
    public int EtsyAccountId { get; set; }
    public virtual EtsyAccount EtsyAccount { get; set; } = null!;

    public long ReceiptId { get; set; } // Etsy Receipt ID
    public long? BuyerUserId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;

    // Financials
    public decimal GrandTotalAmount { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";

    // Status flags
    public bool IsPaid { get; set; }
    public bool IsShipped { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Open;
    public CustomOrderStatus CustomStatus { get; set; } = CustomOrderStatus.New;

    // Dates
    public DateTime OrderDateUtc { get; set; }
    public DateTime? PaidDateUtc { get; set; }
    public DateTime? ShippedDateUtc { get; set; }
    public DateTime? ExpectedShipDateUtc { get; set; }

    // Shipping Destination
    public string? RecipientName { get; set; }
    public string? ShippingFirstLine { get; set; }
    public string? ShippingSecondLine { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingZip { get; set; }
    public string? ShippingCountry { get; set; }
    public string? ShippingCountryIso { get; set; }
    public string? ShippingAddressFormatted { get; set; }

    // Messages & Notes
    public string? MessageFromBuyer { get; set; }
    public string? MessageFromPayment { get; set; }
    public string? InternalNote { get; set; }
    public string? Tags { get; set; } // Comma separated custom tags e.g. "VIP, Urgent, Gift"
    public bool IsGift { get; set; }
    public string? GiftMessage { get; set; }

    public string? RawJson { get; set; }

    // Navigation properties
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public virtual ICollection<OrderTracking> Trackings { get; set; } = new List<OrderTracking>();
}
