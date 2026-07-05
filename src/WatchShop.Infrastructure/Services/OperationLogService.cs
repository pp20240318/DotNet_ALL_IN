using SqlSugar;
using WatchShop.Domain.Entities;

namespace WatchShop.Infrastructure.Services;

public class OperationLogService
{
    private readonly ISqlSugarClient _db;

    public OperationLogService(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task WriteAsync(OperationLog log, CancellationToken cancellationToken = default)
    {
        log.Id = SnowFlakeSingle.Instance.NextId();
        log.CreatedAt = DateTime.UtcNow;
        log.IsDeleted = false;
        log.Version = 0;
        await _db.Insertable(log).ExecuteCommandAsync();
    }
}
