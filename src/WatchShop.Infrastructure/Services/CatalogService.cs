using WatchShop.Application.Abstractions;
using WatchShop.Application.Contracts.Persistence;
using WatchShop.Application.Events;
using WatchShop.Application.Exceptions;
using WatchShop.Application.Features.Catalog;
using WatchShop.Application.Features.Catalog.Dtos;
using WatchShop.Domain.Entities;
using WatchShop.Domain.Enums;
using WatchShop.Infrastructure.Caching;

namespace WatchShop.Infrastructure.Services;

public class CatalogService : ICatalogService
{
    private const string BrandsCacheKey = "catalog:brands";
    private const string CategoriesCacheKey = "catalog:categories";
    private const string ProductsCacheKey = "catalog:products";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly CacheService _cache;

    public CatalogService(IUnitOfWork unitOfWork, IEventPublisher eventPublisher, CacheService cache)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _cache = cache;
    }

    public async Task<List<CatalogBrandResponse>> GetBrandsAsync(CancellationToken cancellationToken = default)
    {
        var cached = await _cache.GetAsync<List<CatalogBrandResponse>>(BrandsCacheKey, cancellationToken);
        if (cached is not null) return cached;

        var items = await _unitOfWork.Repository<Brand>()
            .GetListAsync(x => x.IsEnabled, cancellationToken);

        var result = items.Select(x => new CatalogBrandResponse
        {
            Id = x.Id,
            Name = x.Name,
            LogoUrl = x.LogoUrl
        }).ToList();

        await _cache.SetAsync(BrandsCacheKey, result, TimeSpan.FromMinutes(30), cancellationToken);
        return result;
    }

    public async Task<List<CatalogCategoryResponse>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var cached = await _cache.GetAsync<List<CatalogCategoryResponse>>(CategoriesCacheKey, cancellationToken);
        if (cached is not null) return cached;

        var items = await _unitOfWork.Repository<Category>()
            .GetListAsync(x => x.IsEnabled, cancellationToken);

        var result = items.Select(x => new CatalogCategoryResponse
        {
            Id = x.Id,
            Name = x.Name
        }).ToList();

        await _cache.SetAsync(CategoriesCacheKey, result, TimeSpan.FromMinutes(30), cancellationToken);
        return result;
    }

    public async Task<List<CatalogProductResponse>> GetOnSaleProductsAsync(CancellationToken cancellationToken = default)
    {
        var cached = await _cache.GetAsync<List<CatalogProductResponse>>(ProductsCacheKey, cancellationToken);
        if (cached is not null) return cached;

        var products = await _unitOfWork.Repository<Product>()
            .GetListAsync(x => x.Status == ProductStatus.OnSale, cancellationToken);

        var result = new List<CatalogProductResponse>();
        foreach (var product in products)
        {
            result.Add(await MapProduct(product, cancellationToken));
        }

        await _cache.SetAsync(ProductsCacheKey, result, TimeSpan.FromMinutes(10), cancellationToken);
        return result;
    }

    public async Task<CatalogProductDetailResponse?> GetProductDetailAsync(long id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"catalog:product:{id}";
        var cached = await _cache.GetAsync<CatalogProductDetailResponse>(cacheKey, cancellationToken);
        if (cached is not null) return cached;

        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id, cancellationToken);
        if (product is null || product.Status != ProductStatus.OnSale)
        {
            return null;
        }

        var baseInfo = await MapProduct(product, cancellationToken);
        var skus = await _unitOfWork.Repository<ProductSku>()
            .GetListAsync(x => x.ProductId == id && x.IsEnabled, cancellationToken);

        var detail = new CatalogProductDetailResponse
        {
            Id = baseInfo.Id,
            Name = baseInfo.Name,
            BrandName = baseInfo.BrandName,
            CategoryName = baseInfo.CategoryName,
            Price = baseInfo.Price,
            Status = baseInfo.Status,
            CoverImage = baseInfo.CoverImage,
            Description = product.Description,
            Skus = skus.Select(x => new CatalogSkuResponse
            {
                Id = x.Id,
                SkuCode = x.SkuCode,
                SpecJson = x.SpecJson,
                Price = x.Price,
                Stock = x.Stock
            }).ToList()
        };

        await _cache.SetAsync(cacheKey, detail, TimeSpan.FromMinutes(10), cancellationToken);
        return detail;
    }

    public async Task<long> CreateStoreOrderAsync(
        StoreOrderCreateRequest request,
        long? customerId = null,
        CancellationToken cancellationToken = default)
    {
        if (request.Quantity <= 0)
        {
            throw new BusinessException("购买数量必须大于 0");
        }

        long orderId = 0;
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var sku = await _unitOfWork.Repository<ProductSku>().GetByIdAsync(request.SkuId, cancellationToken)
                ?? throw new BusinessException("SKU 不存在");

            if (!sku.IsEnabled || sku.Stock < request.Quantity)
            {
                throw new BusinessException("库存不足");
            }

            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(sku.ProductId, cancellationToken)
                ?? throw new BusinessException("商品不存在");

            if (product.Status != ProductStatus.OnSale)
            {
                throw new BusinessException("商品未上架");
            }

            sku.Stock -= request.Quantity;
            await _unitOfWork.Repository<ProductSku>().UpdateAsync(sku, cancellationToken);

            var order = new ShopOrder
            {
                OrderNo = $"WS{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}",
                Status = OrderStatus.PendingPayment,
                TotalAmount = sku.Price * request.Quantity,
                CustomerId = customerId,
                ReceiverName = request.ReceiverName,
                ReceiverPhone = request.ReceiverPhone,
                ReceiverAddress = request.ReceiverAddress
            };
            orderId = await _unitOfWork.Repository<ShopOrder>().InsertAsync(order, cancellationToken);

            await _unitOfWork.Repository<OrderItem>().InsertAsync(new OrderItem
            {
                OrderId = orderId,
                ProductId = product.Id,
                SkuId = sku.Id,
                ProductName = product.Name,
                SkuCode = sku.SkuCode,
                Price = sku.Price,
                Quantity = request.Quantity
            }, cancellationToken);

            await _eventPublisher.PublishAsync(
                new OrderCreatedEvent(orderId, order.OrderNo, order.CreatedAt),
                cancellationToken);
        }, cancellationToken);

        await _cache.RemoveAsync(ProductsCacheKey, cancellationToken);
        return orderId;
    }

    private async Task<CatalogProductResponse> MapProduct(Product product, CancellationToken cancellationToken)
    {
        var brand = await _unitOfWork.Repository<Brand>().GetByIdAsync(product.BrandId, cancellationToken);
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(product.CategoryId, cancellationToken);

        return new CatalogProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            BrandName = brand?.Name,
            CategoryName = category?.Name,
            Price = product.Price,
            Status = product.Status,
            CoverImage = product.CoverImage
        };
    }
}
