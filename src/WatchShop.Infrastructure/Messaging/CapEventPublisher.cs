using DotNetCore.CAP;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Events;

namespace WatchShop.Infrastructure.Messaging;

public class CapEventPublisher : IEventPublisher
{
    private readonly ICapPublisher _capPublisher;

    public CapEventPublisher(ICapPublisher capPublisher)
    {
        _capPublisher = capPublisher;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        var topic = @event switch
        {
            OrderCreatedEvent => "watchshop.order.created",
            OrderCancelledEvent => "watchshop.order.cancelled",
            _ => $"watchshop.{@event.GetType().Name}"
        };

        return _capPublisher.PublishAsync(topic, @event, cancellationToken: cancellationToken);
    }
}
