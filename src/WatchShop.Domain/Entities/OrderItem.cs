using SqlSugar;

namespace WatchShop.Domain.Entities;

[SugarTable("order_item")]
public class OrderItem : BaseEntity
{
    public long OrderId { get; set; }

    public long ProductId { get; set; }

    public long SkuId { get; set; }

    [SugarColumn(Length = 200, IsNullable = false)]
    public string ProductName { get; set; } = string.Empty;

    [SugarColumn(Length = 50, IsNullable = false)]
    public string SkuCode { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Quantity { get; set; }
}
