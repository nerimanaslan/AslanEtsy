using AslanEtsy.Application.DTOs.Sync;

namespace AslanEtsy.Application.Interfaces;

public interface IEtsySyncService
{
    Task<SyncResultDto> SyncAccountOrdersAsync(int accountId, CancellationToken cancellationToken = default);
    Task<List<SyncResultDto>> SyncAllActiveAccountsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SyncLogDto>> GetSyncLogsAsync(int? accountId = null, int limit = 50, CancellationToken cancellationToken = default);
}
