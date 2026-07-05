using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        services.Configure<RedisOptions>(
            configuration.GetSection(RedisOptions.SectionName));
        services.Configure<OrderOptions>(
            configuration.GetSection(OrderOptions.SectionName));

        return services;
    }
}
