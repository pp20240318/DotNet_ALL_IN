using WatchShop.Application.Features.Catalog.Dtos;

namespace WatchShop.Application.Features.Catalog;

public interface ICatalogService
{
    Task<List<CatalogBrandResponse>> GetBrandsAsync(CancellationToken cancellationToken = default);
    Task<List<CatalogCategoryResponse>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<List<CatalogProductResponse>> GetOnSaleProductsAsync(CancellationToken cancellationToken = default);
    Task<CatalogProductDetailResponse?> GetProductDetailAsync(long id, CancellationToken cancellationToken = default);
    Task<long> CreateStoreOrderAsync(StoreOrderCreateRequest request, long? customerId = null, CancellationToken cancellationToken = default);
}
