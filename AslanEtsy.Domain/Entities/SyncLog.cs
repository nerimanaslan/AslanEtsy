using AslanEtsy.Domain.Common;
using AslanEtsy.Domain.Enums;

namespace AslanEtsy.Domain.Entities;

public class SyncLog : BaseEntity
{
    public int EtsyAccountId { get; set; }
    public virtual EtsyAccount EtsyAccount { get; set; } = null!;

    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
    public SyncStatus Status { get; set; } = SyncStatus.InProgress;
    
    public int OrdersFetched { get; set; }
    public int OrdersCreated { get; set; }
    public int OrdersUpdated { get; set; }
    
    public string? ErrorMessage { get; set; }
    public string? Details { get; set; }
}
