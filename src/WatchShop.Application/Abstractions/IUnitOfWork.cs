using WatchShop.Domain.Entities;

namespace WatchShop.Application.Abstractions;

public interface IUnitOfWork
{
    IRepository<T> Repository<T>() where T : BaseEntity, new();

    Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
}
