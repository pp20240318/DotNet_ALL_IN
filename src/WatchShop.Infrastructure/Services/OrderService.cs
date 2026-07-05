using SqlSugar;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Common;
using WatchShop.Application.Events;
using WatchShop.Application.Exceptions;
using WatchShop.Application.Options;
using WatchShop.Domain.Entities;
using WatchShop.Domain.Enums;
using Microsoft.Extensions.Options;

namespace WatchShop.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ISqlSugarClient _db;

    public OrderService(IUnitOfWork unitOfWork, IEventPublisher eventPublisher, ISqlSugarClient db)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _db = db;
    }

    public async Task<PagedResult<OrderListResponse>> GetPagedAsync(OrderQueryRequest query, CancellationToken cancellationToken = default)
    {
        var paged = await _unitOfWork.Repository<ShopOrder>().GetPagedAsync(
            query.Page,
            query.PageSize,
            x =>
                (string.IsNullOrWhiteSpace(query.OrderNo) || x.OrderNo.Contains(query.OrderNo!)) &&
                (!query.Status.HasValue || x.Status == query.Status),
            cancellationToken);

        return new PagedResult<OrderListResponse>
        {
            Page = paged.Page,
            PageSize = paged.PageSize,
            Total = paged.Total,
            Items = paged.Items.Select(MapList).ToList()
        };
    }

    public async Task<OrderDetailResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Repository<ShopOrder>().GetByIdAsync(id, cancellationToken);
        if (order is null) return null;

        var items = await _db.Queryable<OrderItem>()
            .Where(x => x.OrderId == id && !x.IsDeleted)
            .ToListAsync();

        var detail = MapDetail(order);
        detail.Items = items.Select(x => new OrderItemResponse
        {
            ProductId = x.ProductId,
            SkuId = x.SkuId,
            ProductName = x.ProductName,
            SkuCode = x.SkuCode,
            Price = x.Price,
            Quantity = x.Quantity
        }).ToList();
        return detail;
    }

    public async Task ShipAsync(long id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<ShopOrder>();
        var order = await repo.GetByIdAsync(id, cancellationToken)
            ?? throw new BusinessException("订单不存在");

        if (order.Status != OrderStatus.Paid)
        {
            throw new BusinessException("只有已支付订单才能发货");
        }

        order.Status = OrderStatus.Shipped;
        order.ShippedAt = DateTime.UtcNow;
        await repo.UpdateAsync(order, cancellationToken);
    }

    public async Task CancelAsync(long id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<ShopOrder>();
        var order = await repo.GetByIdAsync(id, cancellationToken)
            ?? throw new BusinessException("订单不存在");

        if (order.Status is OrderStatus.Shipped or OrderStatus.Completed or OrderStatus.Cancelled)
        {
            throw new BusinessException("当前状态不可取消");
        }

        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;
        await repo.UpdateAsync(order, cancellationToken);
        await _eventPublisher.PublishAsync(new OrderCancelledEvent(order.Id, order.OrderNo, "管理员取消"), cancellationToken);
    }

    public async Task<long> CreateDemoOrderAsync(CancellationToken cancellationToken = default)
    {
        var product = (await _unitOfWork.Repository<Product>().GetListAsync(cancellationToken: cancellationToken)).FirstOrDefault()
            ?? throw new BusinessException("请先创建商品后再生成演示订单");

        var sku = (await _unitOfWork.Repository<ProductSku>().GetListAsync(x => x.ProductId == product.Id, cancellationToken)).FirstOrDefault();
        if (sku is null)
        {
            throw new BusinessException("请先创建 SKU 后再生成演示订单");
        }

        long orderId = 0;
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var order = new ShopOrder
            {
                OrderNo = $"WS{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}",
                Status = OrderStatus.PendingPayment,
                TotalAmount = sku.Price,
                ReceiverName = "演示用户",
                ReceiverPhone = "13800000000",
                ReceiverAddress = "演示地址"
            };
            orderId = await _unitOfWork.Repository<ShopOrder>().InsertAsync(order, cancellationToken);

            var item = new OrderItem
            {
                OrderId = orderId,
                ProductId = product.Id,
                SkuId = sku.Id,
                ProductName = product.Name,
                SkuCode = sku.SkuCode,
                Price = sku.Price,
                Quantity = 1
            };
            await _unitOfWork.Repository<OrderItem>().InsertAsync(item, cancellationToken);
            await _eventPublisher.PublishAsync(new OrderCreatedEvent(orderId, order.OrderNo, order.CreatedAt), cancellationToken);
        }, cancellationToken);

        return orderId;
    }

    private static OrderListResponse MapList(ShopOrder order) => new()
    {
        Id = order.Id,
        OrderNo = order.OrderNo,
        Status = order.Status,
        TotalAmount = order.TotalAmount,
        CreatedAt = order.CreatedAt
    };

    private static OrderDetailResponse MapDetail(ShopOrder order) => new()
    {
        Id = order.Id,
        OrderNo = order.OrderNo,
        Status = order.Status,
        TotalAmount = order.TotalAmount,
        CreatedAt = order.CreatedAt,
        ReceiverName = order.ReceiverName,
        ReceiverPhone = order.ReceiverPhone,
        ReceiverAddress = order.ReceiverAddress,
        PaidAt = order.PaidAt,
        ShippedAt = order.ShippedAt
    };
}
