using AslanEtsy.Domain.Enums;

namespace AslanEtsy.Application.DTOs.Sync;

public class SyncResultDto
{
    public int EtsyAccountId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public SyncStatus Status { get; set; }
    public int OrdersFetched { get; set; }
    public int OrdersCreated { get; set; }
    public int OrdersUpdated { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CompletedAtUtc { get; set; } = DateTime.UtcNow;
}

public class SyncLogDto
{
    public int Id { get; set; }
    public int EtsyAccountId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public SyncStatus Status { get; set; }
    public int OrdersFetched { get; set; }
    public int OrdersCreated { get; set; }
    public int OrdersUpdated { get; set; }
    public string? ErrorMessage { get; set; }
}
