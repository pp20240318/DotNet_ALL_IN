using WatchShop.Application.Abstractions;
using WatchShop.Application.Exceptions;
using WatchShop.Domain.Entities;

namespace WatchShop.Infrastructure.Services;

public class ProductSkuService : IProductSkuService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductSkuService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<long> CreateAsync(ProductSkuCreateRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProductExists(request.ProductId, cancellationToken);

        var entity = new ProductSku
        {
            ProductId = request.ProductId,
            SkuCode = request.SkuCode,
            SpecJson = request.SpecJson,
            Price = request.Price,
            Stock = request.Stock,
            IsEnabled = request.IsEnabled
        };
        return await _unitOfWork.Repository<ProductSku>().InsertAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(long id, ProductSkuUpdateRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProductExists(request.ProductId, cancellationToken);

        var repo = _unitOfWork.Repository<ProductSku>();
        var entity = await repo.GetByIdAsync(id, cancellationToken)
            ?? throw new BusinessException("SKU 不存在");

        entity.ProductId = request.ProductId;
        entity.SkuCode = request.SkuCode;
        entity.SpecJson = request.SpecJson;
        entity.Price = request.Price;
        entity.Stock = request.Stock;
        entity.IsEnabled = request.IsEnabled;
        entity.Version = request.Version;
        await repo.UpdateAsync(entity, cancellationToken);
    }

    public Task DeleteAsync(long id, CancellationToken cancellationToken = default)
        => _unitOfWork.Repository<ProductSku>().SoftDeleteAsync(id, cancellationToken);

    public async Task StockInAsync(long id, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0) throw new BusinessException("入库数量必须大于 0");
        await ChangeStockAsync(id, quantity, cancellationToken);
    }

    public async Task StockOutAsync(long id, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0) throw new BusinessException("出库数量必须大于 0");
        await ChangeStockAsync(id, -quantity, cancellationToken);
    }

    public async Task<ProductSkuResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<ProductSku>().GetByIdAsync(id, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    public async Task<List<ProductSkuResponse>> GetByProductIdAsync(long productId, CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Repository<ProductSku>()
            .GetListAsync(x => x.ProductId == productId, cancellationToken);
        return items.Select(Map).ToList();
    }

    private async Task ChangeStockAsync(long id, int delta, CancellationToken cancellationToken)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var repo = _unitOfWork.Repository<ProductSku>();
            var entity = await repo.GetByIdAsync(id, cancellationToken)
                ?? throw new BusinessException("SKU 不存在");

            if (entity.Stock + delta < 0)
            {
                throw new BusinessException("库存不足");
            }

            entity.Stock += delta;
            await repo.UpdateAsync(entity, cancellationToken);
        }, cancellationToken);
    }

    private async Task EnsureProductExists(long productId, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Repository<Product>().GetByIdAsync(productId, cancellationToken) is null)
        {
            throw new BusinessException("商品不存在");
        }
    }

    private static ProductSkuResponse Map(ProductSku entity) => new()
    {
        Id = entity.Id,
        ProductId = entity.ProductId,
        SkuCode = entity.SkuCode,
        SpecJson = entity.SpecJson,
        Price = entity.Price,
        Stock = entity.Stock,
        IsEnabled = entity.IsEnabled,
        Version = entity.Version
    };
}
