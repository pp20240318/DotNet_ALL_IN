using Microsoft.Extensions.Logging;
using SqlSugar;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Events;
using WatchShop.Domain.Entities;

namespace WatchShop.Infrastructure.Messaging.Handlers;

public class OrderCreatedEventHandler : IIntegrationEventHandler<OrderCreatedEvent>
{
    private readonly ISqlSugarClient _db;
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(ISqlSugarClient db, ILogger<OrderCreatedEventHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Order created: {OrderNo} (Id={OrderId})", @event.OrderNo, @event.OrderId);

        await _db.Insertable(new Notification
        {
            Id = SnowFlakeSingle.Instance.NextId(),
            Title = "新订单",
            Content = $"订单 {@event.OrderNo} 已创建，等待支付",
            Category = "order",
            RelatedId = @event.OrderId,
            IsRead = false,
            IsDeleted = false,
            Version = 0,
            CreatedAt = DateTime.UtcNow
        }).ExecuteCommandAsync();
    }
}

public class OrderCancelledEventHandler : IIntegrationEventHandler<OrderCancelledEvent>
{
    private readonly ISqlSugarClient _db;
    private readonly ILogger<OrderCancelledEventHandler> _logger;

    public OrderCancelledEventHandler(ISqlSugarClient db, ILogger<OrderCancelledEventHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task HandleAsync(OrderCancelledEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Order cancelled: {OrderNo}, reason={Reason}", @event.OrderNo, @event.Reason);

        await _db.Insertable(new Notification
        {
            Id = SnowFlakeSingle.Instance.NextId(),
            Title = "订单取消",
            Content = $"订单 {@event.OrderNo} 已取消：{@event.Reason}",
            Category = "order",
            RelatedId = @event.OrderId,
            IsRead = false,
            IsDeleted = false,
            Version = 0,
            CreatedAt = DateTime.UtcNow
        }).ExecuteCommandAsync();
    }
}
