using WatchShop.Application.Common;
using WatchShop.Domain.Enums;

namespace WatchShop.Application.Abstractions;

public interface IProductService
{
    Task<long> CreateAsync(ProductCreateRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(long id, ProductUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task ChangeStatusAsync(long id, ProductStatus status, CancellationToken cancellationToken = default);
    Task<ProductResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductResponse>> GetPagedAsync(ProductQueryRequest query, CancellationToken cancellationToken = default);
}

public class ProductCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public long BrandId { get; set; }
    public long CategoryId { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    public string? CoverImage { get; set; }
}

public class ProductUpdateRequest : ProductCreateRequest
{
    public int Version { get; set; }
}

public class ProductQueryRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Keyword { get; set; }
    public long? BrandId { get; set; }
    public long? CategoryId { get; set; }
    public ProductStatus? Status { get; set; }
}

public class ProductResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long BrandId { get; set; }
    public string? BrandName { get; set; }
    public long CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public ProductStatus Status { get; set; }
    public string? CoverImage { get; set; }
    public int Version { get; set; }
}
