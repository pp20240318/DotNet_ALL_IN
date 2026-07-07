using Microsoft.AspNetCore.SignalR;
using WatchShop.Admin.Api.Hubs;
using WatchShop.Application.Abstractions;

namespace WatchShop.Admin.Api.Services;

public class SignalRNotificationPushService : INotificationPushService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationPushService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task PushAsync(NotificationPushMessage message, CancellationToken cancellationToken = default)
        => _hubContext.Clients
            .Group(NotificationHub.AdminGroup)
            .SendAsync("ReceiveNotification", message, cancellationToken);
}
