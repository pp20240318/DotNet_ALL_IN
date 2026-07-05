namespace WatchShop.Application.Features.Catalog;

public interface ICatalogCacheInvalidator
{
    Task InvalidateAllAsync(CancellationToken cancellationToken = default);
    Task InvalidateProductAsync(long productId, CancellationToken cancellationToken = default);
}
