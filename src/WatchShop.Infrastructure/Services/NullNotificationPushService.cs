using WatchShop.Application.Abstractions;

namespace WatchShop.Infrastructure.Services;

public class NullNotificationPushService : INotificationPushService
{
    public Task PushAsync(NotificationPushMessage message, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
