using SqlSugar;

namespace WatchShop.Domain.Entities;

[SugarTable("product_sku")]
public class ProductSku : BaseEntity
{
    public long ProductId { get; set; }

    [SugarColumn(Length = 50, IsNullable = false)]
    public string SkuCode { get; set; } = string.Empty;

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? SpecJson { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public bool IsEnabled { get; set; } = true;
}
