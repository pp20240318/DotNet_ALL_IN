using MediatR;
using WatchShop.Application.Common;
using WatchShop.Application.Features.Dashboard;
using WatchShop.Application.Features.OperationLogs;
using WatchShop.Application.Features.Store.Dtos;

namespace WatchShop.Application.Features.AdminQueries;

public record GetDashboardStatsQuery() : IRequest<DashboardStatsResponse>;
public record GetOperationLogsPagedQuery(int Page = 1, int PageSize = 20, string? Module = null)
    : IRequest<PagedResult<OperationLogResponse>>;

public class GetDashboardStatsQueryHandler(IDashboardService service)
    : IRequestHandler<GetDashboardStatsQuery, DashboardStatsResponse>
{
    public Task<DashboardStatsResponse> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        => service.GetStatsAsync(cancellationToken);
}

public class GetOperationLogsPagedQueryHandler(IOperationLogQueryService service)
    : IRequestHandler<GetOperationLogsPagedQuery, PagedResult<OperationLogResponse>>
{
    public Task<PagedResult<OperationLogResponse>> Handle(GetOperationLogsPagedQuery request, CancellationToken cancellationToken)
        => service.GetPagedAsync(request.Page, request.PageSize, request.Module, cancellationToken);
}
