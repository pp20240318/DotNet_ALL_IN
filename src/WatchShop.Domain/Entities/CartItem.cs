using SqlSugar;

namespace WatchShop.Domain.Entities;

[SugarTable("cart_item")]
public class CartItem : BaseEntity
{
    public long CustomerId { get; set; }

    public long SkuId { get; set; }

    public int Quantity { get; set; } = 1;
}
