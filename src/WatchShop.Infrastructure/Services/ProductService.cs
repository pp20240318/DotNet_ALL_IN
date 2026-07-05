using WatchShop.Application.Abstractions;
using WatchShop.Application.Contracts.Persistence;
using WatchShop.Application.Common;
using WatchShop.Application.Exceptions;
using WatchShop.Domain.Entities;
using WatchShop.Domain.Enums;
using WatchShop.Application.Features.Catalog;
using WatchShop.Infrastructure.Caching;

namespace WatchShop.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly CacheService _cache;
    private readonly ICatalogCacheInvalidator _catalogCacheInvalidator;

    public ProductService(IUnitOfWork unitOfWork, CacheService cache, ICatalogCacheInvalidator catalogCacheInvalidator)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _catalogCacheInvalidator = catalogCacheInvalidator;
    }

    public async Task<long> CreateAsync(ProductCreateRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureBrandAndCategoryExist(request.BrandId, request.CategoryId, cancellationToken);

        var entity = new Product
        {
            Name = request.Name,
            BrandId = request.BrandId,
            CategoryId = request.CategoryId,
            Description = request.Description,
            Price = request.Price,
            Status = request.Status,
            CoverImage = request.CoverImage
        };
        var id = await _unitOfWork.Repository<Product>().InsertAsync(entity, cancellationToken);
        await _catalogCacheInvalidator.InvalidateAllAsync(cancellationToken);
        return id;
    }

    public async Task UpdateAsync(long id, ProductUpdateRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureBrandAndCategoryExist(request.BrandId, request.CategoryId, cancellationToken);

        var repo = _unitOfWork.Repository<Product>();
        var entity = await repo.GetByIdAsync(id, cancellationToken)
            ?? throw new BusinessException("商品不存在");

        entity.Name = request.Name;
        entity.BrandId = request.BrandId;
        entity.CategoryId = request.CategoryId;
        entity.Description = request.Description;
        entity.Price = request.Price;
        entity.Status = request.Status;
        entity.CoverImage = request.CoverImage;
        entity.Version = request.Version;
        await repo.UpdateAsync(entity, cancellationToken);
        await _cache.RemoveAsync(GetCacheKey(id), cancellationToken);
        await _catalogCacheInvalidator.InvalidateProductAsync(id, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.Repository<Product>().SoftDeleteAsync(id, cancellationToken);
        await _cache.RemoveAsync(GetCacheKey(id), cancellationToken);
        await _catalogCacheInvalidator.InvalidateProductAsync(id, cancellationToken);
    }

    public async Task ChangeStatusAsync(long id, ProductStatus status, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Product>();
        var entity = await repo.GetByIdAsync(id, cancellationToken)
            ?? throw new BusinessException("商品不存在");

        entity.Status = status;
        await repo.UpdateAsync(entity, cancellationToken);
        await _cache.RemoveAsync(GetCacheKey(id), cancellationToken);
        await _catalogCacheInvalidator.InvalidateProductAsync(id, cancellationToken);
    }

    public async Task<ProductResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var cached = await _cache.GetAsync<ProductResponse>(GetCacheKey(id), cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var response = await QueryProductResponse(id, cancellationToken);
        if (response is not null)
        {
            await _cache.SetAsync(GetCacheKey(id), response, cancellationToken: cancellationToken);
        }

        return response;
    }

    public async Task<PagedResult<ProductResponse>> GetPagedAsync(ProductQueryRequest query, CancellationToken cancellationToken = default)
    {
        var paged = await _unitOfWork.Repository<Product>().GetPagedAsync(
            query.Page,
            query.PageSize,
            x =>
                (string.IsNullOrWhiteSpace(query.Keyword) || x.Name.Contains(query.Keyword!)) &&
                (!query.BrandId.HasValue || x.BrandId == query.BrandId) &&
                (!query.CategoryId.HasValue || x.CategoryId == query.CategoryId) &&
                (!query.Status.HasValue || x.Status == query.Status),
            cancellationToken);

        var items = new List<ProductResponse>();
        foreach (var product in paged.Items)
        {
            items.Add(await MapProduct(product, cancellationToken));
        }

        return new PagedResult<ProductResponse>
        {
            Page = paged.Page,
            PageSize = paged.PageSize,
            Total = paged.Total,
            Items = items
        };
    }

    private async Task EnsureBrandAndCategoryExist(long brandId, long categoryId, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Repository<Brand>().GetByIdAsync(brandId, cancellationToken) is null)
        {
            throw new BusinessException("品牌不存在");
        }

        if (await _unitOfWork.Repository<Category>().GetByIdAsync(categoryId, cancellationToken) is null)
        {
            throw new BusinessException("分类不存在");
        }
    }

    private async Task<ProductResponse?> QueryProductResponse(long id, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.Repository<Product>().GetByIdAsync(id, cancellationToken);
        return entity is null ? null : await MapProduct(entity, cancellationToken);
    }

    private async Task<ProductResponse> MapProduct(Product entity, CancellationToken cancellationToken)
    {
        var brand = await _unitOfWork.Repository<Brand>().GetByIdAsync(entity.BrandId, cancellationToken);
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(entity.CategoryId, cancellationToken);

        return new ProductResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            BrandId = entity.BrandId,
            BrandName = brand?.Name,
            CategoryId = entity.CategoryId,
            CategoryName = category?.Name,
            Description = entity.Description,
            Price = entity.Price,
            Status = entity.Status,
            CoverImage = entity.CoverImage,
            Version = entity.Version
        };
    }

    private static string GetCacheKey(long id) => $"product:{id}";
}
