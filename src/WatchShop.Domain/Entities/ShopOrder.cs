using SqlSugar;
using WatchShop.Domain.Enums;

namespace WatchShop.Domain.Entities;

[SugarTable("shop_order")]
public class ShopOrder : BaseEntity
{
    [SugarColumn(Length = 32, IsNullable = false)]
    public string OrderNo { get; set; } = string.Empty;

    public OrderStatus Status { get; set; } = OrderStatus.PendingPayment;

    public decimal TotalAmount { get; set; }

    [SugarColumn(Length = 50, IsNullable = true)]
    public string? ReceiverName { get; set; }

    [SugarColumn(Length = 20, IsNullable = true)]
    public string? ReceiverPhone { get; set; }

    [SugarColumn(Length = 300, IsNullable = true)]
    public string? ReceiverAddress { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime? ShippedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? CancelledAt { get; set; }
}
