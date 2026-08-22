using AslanEtsy.Domain.Common;

namespace AslanEtsy.Domain.Entities;

public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public virtual Order Order { get; set; } = null!;

    public long TransactionId { get; set; } // Etsy Transaction ID
    public long ListingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string? Sku { get; set; }
    public string? ImageUrl { get; set; }
    
    // Etsy variations / selections stored as formatted JSON/text (e.g. Size: Large, Color: Black)
    public string? VariationsJson { get; set; }
    public string? VariationsSummary { get; set; }

    public bool IsCustomOrder { get; set; }
    public string? BuyerPersonalization { get; set; }
}
