using DotNetCore.CAP;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Events;

namespace WatchShop.Infrastructure.Messaging;

public class OrderCreatedCapSubscriber : ICapSubscribe
{
    private readonly IIntegrationEventHandler<OrderCreatedEvent> _handler;

    public OrderCreatedCapSubscriber(IIntegrationEventHandler<OrderCreatedEvent> handler)
    {
        _handler = handler;
    }

    [CapSubscribe("watchshop.order.created")]
    public Task Handle(OrderCreatedEvent @event, CancellationToken cancellationToken = default)
        => _handler.HandleAsync(@event, cancellationToken);
}

public class OrderCancelledCapSubscriber : ICapSubscribe
{
    private readonly IIntegrationEventHandler<OrderCancelledEvent> _handler;

    public OrderCancelledCapSubscriber(IIntegrationEventHandler<OrderCancelledEvent> handler)
    {
        _handler = handler;
    }

    [CapSubscribe("watchshop.order.cancelled")]
    public Task Handle(OrderCancelledEvent @event, CancellationToken cancellationToken = default)
        => _handler.HandleAsync(@event, cancellationToken);
}
