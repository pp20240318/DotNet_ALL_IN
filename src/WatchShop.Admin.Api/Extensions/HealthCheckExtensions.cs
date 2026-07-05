using Microsoft.Extensions.Diagnostics.HealthChecks;
using WatchShop.Application.Options;

namespace WatchShop.Admin.Api.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddWatchShopHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var dbOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? throw new InvalidOperationException("Database options are not configured.");
        var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>() ?? new RedisOptions();

        var builder = services.AddHealthChecks()
            .AddMySql(dbOptions.BuildConnectionString(), name: "mysql");

        if (redisOptions.Enabled)
        {
            builder.AddRedis(redisOptions.ConnectionString, name: "redis");
        }

        return services;
    }

    public static WebApplication MapWatchShopHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready") || check.Name is "mysql" or "redis"
        });
        return app;
    }
}
