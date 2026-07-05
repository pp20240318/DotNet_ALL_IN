using SqlSugar;
using WatchShop.Application.Common;
using WatchShop.Application.Features.OperationLogs;
using WatchShop.Application.Features.Store.Dtos;
using WatchShop.Domain.Entities;

namespace WatchShop.Infrastructure.Services;

public class OperationLogQueryService : IOperationLogQueryService
{
    private readonly ISqlSugarClient _db;

    public OperationLogQueryService(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<PagedResult<OperationLogResponse>> GetPagedAsync(
        int page, int pageSize, string? module = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Queryable<OperationLog>().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(module))
        {
            query = query.Where(x => x.Module == module);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<OperationLogResponse>
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items.Select(x => new OperationLogResponse
            {
                Id = x.Id,
                AdminName = x.AdminName,
                Module = x.Module,
                Action = x.Action,
                RequestPath = x.RequestPath,
                RequestMethod = x.RequestMethod,
                IsSuccess = x.IsSuccess,
                CreatedAt = x.CreatedAt
            }).ToList()
        };
    }
}
