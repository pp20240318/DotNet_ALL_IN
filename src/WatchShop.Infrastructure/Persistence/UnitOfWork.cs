using SqlSugar;
using WatchShop.Application.Abstractions;
using WatchShop.Domain.Entities;

namespace WatchShop.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ISqlSugarClient _db;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(ISqlSugarClient db)
    {
        _db = db;
    }

    public IRepository<T> Repository<T>() where T : BaseEntity, new()
    {
        var type = typeof(T);
        if (!_repositories.TryGetValue(type, out var repository))
        {
            repository = new Repository<T>(_db);
            _repositories[type] = repository;
        }

        return (IRepository<T>)repository;
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        await _db.Ado.UseTranAsync(async () => await action());
    }
}
