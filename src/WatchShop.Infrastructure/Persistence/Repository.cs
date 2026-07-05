using System.Linq.Expressions;
using SqlSugar;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Common;
using WatchShop.Domain.Entities;

namespace WatchShop.Infrastructure.Persistence;

public class Repository<T> : IRepository<T> where T : BaseEntity, new()
{
    private readonly ISqlSugarClient _db;

    public Repository(ISqlSugarClient db)
    {
        _db = db;
    }

    private ISugarQueryable<T> Query(bool includeDeleted = false)
    {
        var query = _db.Queryable<T>();
        return includeDeleted ? query : query.Where(x => !x.IsDeleted);
    }

    public async Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await Query().InSingleAsync(id);
    }

    public async Task<PagedResult<T>> GetPagedAsync(
        int page,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        RefAsync<int> total = 0;
        var query = Query();
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        var items = await query
            .OrderBy(x => x.CreatedAt, OrderByType.Desc)
            .ToPageListAsync(page, pageSize, total);

        return new PagedResult<T>
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items
        };
    }

    public async Task<List<T>> GetListAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var query = Query();
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return await query.OrderBy(x => x.CreatedAt, OrderByType.Desc).ToListAsync();
    }

    public async Task<long> InsertAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.Id = SnowFlakeSingle.Instance.NextId();
        entity.CreatedAt = DateTime.UtcNow;
        entity.Version = 0;
        entity.IsDeleted = false;
        await _db.Insertable(entity).ExecuteCommandAsync();
        return entity.Id;
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        var rows = await _db.Updateable(entity)
            .IgnoreColumns(x => new { x.CreatedAt, x.IsDeleted })
            .ExecuteCommandWithOptLockAsync(true);

        if (rows == 0)
        {
            throw new InvalidOperationException("更新失败，数据可能已被其他人修改。");
        }
    }

    public async Task SoftDeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("记录不存在。");

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.Updateable(entity)
            .UpdateColumns(x => new { x.IsDeleted, x.UpdatedAt, x.Version })
            .ExecuteCommandAsync();
    }
}
