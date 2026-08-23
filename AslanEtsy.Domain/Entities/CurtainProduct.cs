using AslanEtsy.Domain.Common;

namespace AslanEtsy.Domain.Entities;

public class CurtainProduct : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal M2Price { get; set; } = 4000;
    public string Category { get; set; } = "Curtain"; // "Curtain" or "Bedding"
    public string? Fabric { get; set; }
    public string? Note { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
