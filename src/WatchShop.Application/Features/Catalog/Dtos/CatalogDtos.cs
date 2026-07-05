using WatchShop.Domain.Enums;

namespace WatchShop.Application.Features.Catalog.Dtos;

public class CatalogBrandResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
}

public class CatalogCategoryResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CatalogProductResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public string? CategoryName { get; set; }
    public decimal Price { get; set; }
    public ProductStatus Status { get; set; }
    public string? CoverImage { get; set; }
}

public class CatalogProductDetailResponse : CatalogProductResponse
{
    public string? Description { get; set; }
    public List<CatalogSkuResponse> Skus { get; set; } = [];
}

public class CatalogSkuResponse
{
    public long Id { get; set; }
    public string SkuCode { get; set; } = string.Empty;
    public string? SpecJson { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

public class StoreOrderCreateRequest
{
    public long SkuId { get; set; }
    public int Quantity { get; set; } = 1;
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverPhone { get; set; } = string.Empty;
    public string ReceiverAddress { get; set; } = string.Empty;
}
