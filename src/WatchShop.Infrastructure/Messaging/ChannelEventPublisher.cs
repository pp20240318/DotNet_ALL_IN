using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WatchShop.Application.Abstractions;

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

public class EventConsumerBackgroundService : BackgroundService
{
    private readonly Channel<object> _channel;
    private readonly ILogger<EventConsumerBackgroundService> _logger;

    public EventConsumerBackgroundService(
        Channel<object> channel,
        ILogger<EventConsumerBackgroundService> logger)
    {
        _channel = channel;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var @event in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            _logger.LogInformation("Event consumed: {EventType} {@Event}", @event.GetType().Name, @event);
        }
    }
}
