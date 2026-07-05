using System.Linq.Expressions;
using WatchShop.Application.Common;
using WatchShop.Domain.Entities;

namespace WatchShop.Application.Contracts.Persistence;

public interface IRepository<T> where T : BaseEntity, new()
{
    Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<PagedResult<T>> GetPagedAsync(
        int page,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    Task<List<T>> GetListAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    Task<long> InsertAsync(T entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(long id, CancellationToken cancellationToken = default);
}
