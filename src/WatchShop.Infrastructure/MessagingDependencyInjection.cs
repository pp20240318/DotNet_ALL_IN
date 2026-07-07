using System.Threading.Channels;
using DotNetCore.CAP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Events;
using WatchShop.Application.Options;
using WatchShop.Infrastructure.Messaging;
using WatchShop.Infrastructure.Messaging.Handlers;

namespace WatchShop.Infrastructure;

public static class MessagingDependencyInjection
{
    public static IServiceCollection AddWatchShopMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var messagingOptions = configuration.GetSection(MessagingOptions.SectionName).Get<MessagingOptions>()
            ?? new MessagingOptions();

        services.AddScoped<IIntegrationEventHandler<OrderCreatedEvent>, OrderCreatedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<OrderCancelledEvent>, OrderCancelledEventHandler>();

        if (messagingOptions.UseCap)
        {
            var dbOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
                ?? throw new InvalidOperationException("Database options are not configured.");

            services.AddCap(cap =>
            {
                cap.UseMySql(dbOptions.BuildConnectionString());
                cap.UseRabbitMQ(options =>
                {
                    options.HostName = messagingOptions.RabbitMqHost;
                    options.Port = messagingOptions.RabbitMqPort;
                    options.UserName = messagingOptions.RabbitMqUser;
                    options.Password = messagingOptions.RabbitMqPassword;
                });
                cap.DefaultGroupName = messagingOptions.DefaultGroup;
            });

            services.AddSingleton<IEventPublisher, CapEventPublisher>();
            services.AddScoped<OrderCreatedCapSubscriber>();
            services.AddScoped<OrderCancelledCapSubscriber>();
        }
        else
        {
            services.AddSingleton(_ => Channel.CreateUnbounded<object>());
            services.AddSingleton<IEventPublisher, ChannelEventPublisher>();
            services.AddHostedService<EventDispatcherBackgroundService>();
        }

        return services;
    }
}
