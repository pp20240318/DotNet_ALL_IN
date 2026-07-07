using Hangfire;
using Hangfire.MySql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WatchShop.Application.Options;
using WatchShop.Infrastructure.Background;

namespace WatchShop.Admin.Api.Extensions;

public static class HangfireExtensions
{
    public static IServiceCollection AddWatchShopHangfire(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var hangfireOptions = configuration.GetSection(HangfireOptions.SectionName).Get<HangfireOptions>()
            ?? new HangfireOptions();
        if (!hangfireOptions.Enabled)
        {
            return services;
        }

        var dbOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? throw new InvalidOperationException("Database options are not configured.");

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseStorage(new MySqlStorage(
                dbOptions.BuildConnectionString(),
                new MySqlStorageOptions
                {
                    TablesPrefix = "Hangfire_"
                })));

        services.AddHangfireServer();
        services.AddScoped<OrderTimeoutJob>();
        services.AddScoped<SearchIndexSyncJob>();

        return services;
    }

    public static WebApplication UseWatchShopHangfire(this WebApplication app)
    {
        var hangfireOptions = app.Services.GetRequiredService<IOptions<HangfireOptions>>().Value;
        if (!hangfireOptions.Enabled)
        {
            return app;
        }

        app.UseHangfireDashboard(hangfireOptions.DashboardPath, new DashboardOptions
        {
            Authorization = [new HangfireDashboardAuthorizationFilter()]
        });

        RecurringJob.AddOrUpdate<OrderTimeoutJob>(
            "order-timeout",
            job => job.RunAsync(),
            Cron.Minutely);

        var esOptions = app.Configuration.GetSection(ElasticsearchOptions.SectionName).Get<ElasticsearchOptions>();
        if (esOptions?.Enabled == true)
        {
            RecurringJob.AddOrUpdate<SearchIndexSyncJob>(
                "search-index-sync",
                job => job.RunAsync(),
                "*/5 * * * *");
        }

        return app;
    }
}
