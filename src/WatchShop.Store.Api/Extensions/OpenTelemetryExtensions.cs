using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace WatchShop.Store.Api.Extensions;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddWatchShopOpenTelemetry(
        this IServiceCollection services,
        string serviceName)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("WatchShop.Application")
                .AddConsoleExporter())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddPrometheusExporter());

        return services;
    }

    public static WebApplication MapWatchShopMetrics(this WebApplication app)
    {
        app.MapPrometheusScrapingEndpoint();
        return app;
    }
}
