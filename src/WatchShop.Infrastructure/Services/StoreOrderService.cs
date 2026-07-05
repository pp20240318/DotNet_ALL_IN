using SqlSugar;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Common;
using WatchShop.Application.Contracts.Persistence;
using WatchShop.Application.Events;
using WatchShop.Application.Exceptions;
using WatchShop.Application.Features.Store;
using WatchShop.Application.Features.Store.Dtos;
using WatchShop.Domain.Entities;
using WatchShop.Domain.Enums;

namespace WatchShop.Infrastructure.Services;

public class StoreOrderService : IStoreOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ISqlSugarClient _db;

    public StoreOrderService(IUnitOfWork unitOfWork, IEventPublisher eventPublisher, ISqlSugarClient db)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _db = db;
    }

    public async Task<PagedResult<StoreOrderSummaryResponse>> GetMyOrdersAsync(
        long customerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var paged = await _unitOfWork.Repository<ShopOrder>().GetPagedAsync(
            page, pageSize, x => x.CustomerId == customerId, cancellationToken);

        return new PagedResult<StoreOrderSummaryResponse>
        {
            Page = paged.Page,
            PageSize = paged.PageSize,
            Total = paged.Total,
            Items = paged.Items.Select(MapSummary).ToList()
        };
    }

    public async Task<StoreOrderDetailResponse?> GetMyOrderDetailAsync(
        long customerId, long orderId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Repository<ShopOrder>().GetByIdAsync(orderId, cancellationToken);
        if (order is null || order.CustomerId != customerId)
        {
            return null;
        }

        var items = await _db.Queryable<OrderItem>()
            .Where(x => x.OrderId == orderId && !x.IsDeleted)
            .ToListAsync();

        var detail = MapDetail(order);
        detail.Items = items.Select(x => new StoreOrderItemResponse
        {
            ProductName = x.ProductName,
            SkuCode = x.SkuCode,
            Price = x.Price,
            Quantity = x.Quantity
        }).ToList();
        return detail;
    }

    public async Task PayOrderAsync(long customerId, long orderId, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<ShopOrder>();
        var order = await repo.GetByIdAsync(orderId, cancellationToken)
            ?? throw new BusinessException("订单不存在");

        if (order.CustomerId != customerId)
        {
            throw new BusinessException("无权操作此订单");
        }

        if (order.Status != OrderStatus.PendingPayment)
        {
            throw new BusinessException("订单状态不允许支付");
        }

        order.Status = OrderStatus.Paid;
        order.PaidAt = DateTime.UtcNow;
        await repo.UpdateAsync(order, cancellationToken);
    }

    public async Task CancelOrderAsync(long customerId, long orderId, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<ShopOrder>();
        var order = await repo.GetByIdAsync(orderId, cancellationToken)
            ?? throw new BusinessException("订单不存在");

        if (order.CustomerId != customerId)
        {
            throw new BusinessException("无权操作此订单");
        }

        if (order.Status is OrderStatus.Shipped or OrderStatus.Completed or OrderStatus.Cancelled)
        {
            throw new BusinessException("当前状态不可取消");
        }

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var items = await _db.Queryable<OrderItem>()
                .Where(x => x.OrderId == orderId && !x.IsDeleted)
                .ToListAsync();

            foreach (var item in items)
            {
                var sku = await _unitOfWork.Repository<ProductSku>().GetByIdAsync(item.SkuId, cancellationToken);
                if (sku is not null)
                {
                    sku.Stock += item.Quantity;
                    await _unitOfWork.Repository<ProductSku>().UpdateAsync(sku, cancellationToken);
                }
            }

            order.Status = OrderStatus.Cancelled;
            order.CancelledAt = DateTime.UtcNow;
            await repo.UpdateAsync(order, cancellationToken);
            await _eventPublisher.PublishAsync(
                new OrderCancelledEvent(order.Id, order.OrderNo, "用户取消"),
                cancellationToken);
        }, cancellationToken);
    }

    private static StoreOrderSummaryResponse MapSummary(ShopOrder order) => new()
    {
        Id = order.Id,
        OrderNo = order.OrderNo,
        Status = order.Status,
        TotalAmount = order.TotalAmount,
        CreatedAt = order.CreatedAt
    };

    private static StoreOrderDetailResponse MapDetail(ShopOrder order) => new()
    {
        Id = order.Id,
        OrderNo = order.OrderNo,
        Status = order.Status,
        TotalAmount = order.TotalAmount,
        CreatedAt = order.CreatedAt,
        ReceiverName = order.ReceiverName,
        ReceiverPhone = order.ReceiverPhone,
        ReceiverAddress = order.ReceiverAddress,
        PaidAt = order.PaidAt
    };
}
