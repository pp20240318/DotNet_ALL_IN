using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WatchShop.Application.Options;

namespace WatchShop.Store.Api.Extensions;

public static class StoreHealthExtensions
{
    public static IServiceCollection AddStoreHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var dbOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>();
        var healthChecks = services.AddHealthChecks();

        if (dbOptions is not null && !string.IsNullOrWhiteSpace(dbOptions.Database))
        {
            healthChecks.AddMySql(dbOptions.BuildConnectionString(), name: "mysql");
        }

        return services;
    }

    public static WebApplication MapStoreHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });
        return app;
    }

    public static IServiceCollection AddStoreRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddFixedWindowLimiter("store-fixed", limiter =>
            {
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.PermitLimit = 120;
                limiter.QueueLimit = 0;
            });
        });
        return services;
    }
}
