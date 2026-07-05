using WatchShop.Infrastructure.Caching;

namespace WatchShop.Infrastructure.Caching;

public class CatalogCacheInvalidator : Application.Features.Catalog.ICatalogCacheInvalidator
{
    private readonly CacheService _cache;

    public CatalogCacheInvalidator(CacheService cache)
    {
        _cache = cache;
    }

    public async Task InvalidateAllAsync(CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync("catalog:brands", cancellationToken);
        await _cache.RemoveAsync("catalog:categories", cancellationToken);
        await _cache.RemoveAsync("catalog:products", cancellationToken);
    }

    public async Task InvalidateProductAsync(long productId, CancellationToken cancellationToken = default)
    {
        await InvalidateAllAsync(cancellationToken);
        await _cache.RemoveAsync($"catalog:product:{productId}", cancellationToken);
    }
}
