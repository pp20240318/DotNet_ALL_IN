using WatchShop.Application.Common;
using WatchShop.Application.Features.Store.Dtos;

namespace WatchShop.Application.Features.OperationLogs;

public interface IOperationLogQueryService
{
    Task<PagedResult<OperationLogResponse>> GetPagedAsync(int page, int pageSize, string? module = null, CancellationToken cancellationToken = default);
}
