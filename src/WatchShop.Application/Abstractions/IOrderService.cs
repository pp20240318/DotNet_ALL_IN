using WatchShop.Application.Common;
using WatchShop.Domain.Enums;

namespace WatchShop.Application.Abstractions;

public interface IOrderService
{
    Task<PagedResult<OrderListResponse>> GetPagedAsync(OrderQueryRequest query, CancellationToken cancellationToken = default);
    Task<OrderDetailResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task ShipAsync(long id, CancellationToken cancellationToken = default);
    Task CancelAsync(long id, CancellationToken cancellationToken = default);
    Task RefundAsync(long id, CancellationToken cancellationToken = default);
    Task<long> CreateDemoOrderAsync(CancellationToken cancellationToken = default);
    Task<byte[]> ExportCsvAsync(OrderStatus? status = null, int maxRows = 5000, CancellationToken cancellationToken = default);
}

public class OrderQueryRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? OrderNo { get; set; }
    public OrderStatus? Status { get; set; }
}

public class OrderListResponse
{
    public long Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderDetailResponse : OrderListResponse
{
    public string? ReceiverName { get; set; }
    public string? ReceiverPhone { get; set; }
    public string? ReceiverAddress { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public List<OrderItemResponse> Items { get; set; } = [];
}

public class OrderItemResponse
{
    public long ProductId { get; set; }
    public long SkuId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SkuCode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
