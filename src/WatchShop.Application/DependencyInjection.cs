using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WatchShop.Application.Behaviors;
using WatchShop.Application.Options;

namespace WatchShop.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(
            configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));
        services.Configure<StoreJwtOptions>(
            configuration.GetSection(StoreJwtOptions.SectionName));
        services.Configure<RedisOptions>(
            configuration.GetSection(RedisOptions.SectionName));
        services.Configure<OrderOptions>(
            configuration.GetSection(OrderOptions.SectionName));
        services.Configure<MessagingOptions>(
            configuration.GetSection(MessagingOptions.SectionName));
        services.Configure<FileStorageOptions>(
            configuration.GetSection(FileStorageOptions.SectionName));
        services.Configure<PaymentOptions>(
            configuration.GetSection(PaymentOptions.SectionName));
        services.Configure<MinioOptions>(
            configuration.GetSection(MinioOptions.SectionName));
        services.Configure<ElasticsearchOptions>(
            configuration.GetSection(ElasticsearchOptions.SectionName));
        services.Configure<HangfireOptions>(
            configuration.GetSection(HangfireOptions.SectionName));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        return services;
    }
}
