using WatchShop.Application.Features.Store.Dtos;

namespace WatchShop.Application.Features.Dashboard;

public interface IDashboardService
{
    Task<DashboardStatsResponse> GetStatsAsync(CancellationToken cancellationToken = default);
}
