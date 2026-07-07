using WatchShop.Admin.Api.Hubs;
using WatchShop.Admin.Api.Services;
using WatchShop.Application.Abstractions;

namespace WatchShop.Admin.Api.Extensions;

public static class SignalRExtensions
{
    public static IServiceCollection AddWatchShopSignalR(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<INotificationPushService, SignalRNotificationPushService>();
        return services;
    }

    public static WebApplication MapWatchShopSignalR(this WebApplication app)
    {
        app.MapHub<NotificationHub>("/hubs/notifications");
        return app;
    }
}
