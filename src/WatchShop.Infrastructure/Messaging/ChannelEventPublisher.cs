using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Events;

namespace WatchShop.Infrastructure.Messaging;

public class ChannelEventPublisher : IEventPublisher
{
    private readonly Channel<object> _channel;

    public ChannelEventPublisher(Channel<object> channel)
    {
        _channel = channel;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        await _channel.Writer.WriteAsync(@event, cancellationToken);
    }
}

public class EventDispatcherBackgroundService : BackgroundService
{
    private readonly Channel<object> _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventDispatcherBackgroundService> _logger;

    public EventDispatcherBackgroundService(
        Channel<object> channel,
        IServiceProvider serviceProvider,
        ILogger<EventDispatcherBackgroundService> logger)
    {
        _channel = channel;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var @event in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope = _serviceProvider.CreateScope();
            try
            {
                switch (@event)
                {
                    case OrderCreatedEvent created:
                        await scope.ServiceProvider
                            .GetRequiredService<IIntegrationEventHandler<OrderCreatedEvent>>()
                            .HandleAsync(created, stoppingToken);
                        break;
                    case OrderCancelledEvent cancelled:
                        await scope.ServiceProvider
                            .GetRequiredService<IIntegrationEventHandler<OrderCancelledEvent>>()
                            .HandleAsync(cancelled, stoppingToken);
                        break;
                    default:
                        _logger.LogWarning("No handler registered for event {EventType}", @event.GetType().Name);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch event {EventType}", @event.GetType().Name);
            }
        }
    }
}
