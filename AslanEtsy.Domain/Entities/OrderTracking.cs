using AslanEtsy.Domain.Common;

namespace AslanEtsy.Domain.Entities;

public class OrderTracking : BaseEntity
{
    public int OrderId { get; set; }
    public virtual Order Order { get; set; } = null!;

    public string TrackingCode { get; set; } = string.Empty;
    public string CarrierName { get; set; } = string.Empty; // e.g. "USPS", "FedEx", "DHL", "UPS", "PTT", etc.
    public DateTime ShipDateUtc { get; set; } = DateTime.UtcNow;
    public string? TrackingUrl { get; set; }

    public bool IsSyncedToEtsy { get; set; }
    public DateTime? SyncedToEtsyAtUtc { get; set; }
    public string? EtsySyncErrorMessage { get; set; }
    
    public string? Note { get; set; }
}
