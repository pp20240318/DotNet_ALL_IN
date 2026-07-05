using WatchShop.Application.Common;

namespace WatchShop.Application.Abstractions;

public interface IProductSkuService
{
    Task<long> CreateAsync(ProductSkuCreateRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(long id, ProductSkuUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task StockInAsync(long id, int quantity, CancellationToken cancellationToken = default);
    Task StockOutAsync(long id, int quantity, CancellationToken cancellationToken = default);
    Task<ProductSkuResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<List<ProductSkuResponse>> GetByProductIdAsync(long productId, CancellationToken cancellationToken = default);
}

public class ProductSkuCreateRequest
{
    public long ProductId { get; set; }
    public string SkuCode { get; set; } = string.Empty;
    public string? SpecJson { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public class ProductSkuUpdateRequest : ProductSkuCreateRequest
{
    public int Version { get; set; }
}

public class ProductSkuResponse
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string SkuCode { get; set; } = string.Empty;
    public string? SpecJson { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsEnabled { get; set; }
    public int Version { get; set; }
}
