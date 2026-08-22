using AslanEtsy.Application.DTOs.Dashboard;

namespace AslanEtsy.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
}
