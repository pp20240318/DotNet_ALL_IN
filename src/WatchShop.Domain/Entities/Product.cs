using SqlSugar;
using WatchShop.Domain.Enums;

namespace WatchShop.Domain.Entities;

[SugarTable("product")]
public class Product : BaseEntity
{
    [SugarColumn(Length = 200, IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    public long BrandId { get; set; }

    public long CategoryId { get; set; }

    [SugarColumn(Length = 2000, IsNullable = true)]
    public string? Description { get; set; }

    public decimal Price { get; set; }

    public ProductStatus Status { get; set; } = ProductStatus.Draft;

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? CoverImage { get; set; }
}
