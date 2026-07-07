using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Features.Search;
using WatchShop.Application.Options;
using WatchShop.Infrastructure.Background;
using WatchShop.Infrastructure.Search;
using WatchShop.Infrastructure.Security;
using WatchShop.Infrastructure.Services;
using WatchShop.Infrastructure.Storage;

namespace WatchShop.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddWatchShopStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<LocalFileStorageService>();
        services.AddScoped<MinioFileStorageService>();
        services.AddScoped<IFileStorageService>(sp =>
        {
            var minio = sp.GetRequiredService<IOptions<MinioOptions>>().Value;
            return minio.Enabled
                ? sp.GetRequiredService<MinioFileStorageService>()
                : sp.GetRequiredService<LocalFileStorageService>();
        });

        return services;
    }

    public static IServiceCollection AddWatchShopSearch(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<SearchService>();
        services.AddScoped<ElasticsearchSearchService>();
        services.AddScoped<ISearchService>(sp =>
        {
            var es = sp.GetRequiredService<IOptions<ElasticsearchOptions>>().Value;
            return es.Enabled
                ? sp.GetRequiredService<CompositeSearchService>()
                : sp.GetRequiredService<SearchService>();
        });
        services.AddScoped<CompositeSearchService>();

        return services;
    }

    public static IServiceCollection AddWatchShopBackgroundJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var hangfire = configuration.GetSection(HangfireOptions.SectionName).Get<HangfireOptions>()
            ?? new HangfireOptions();

        if (!hangfire.Enabled)
        {
            services.AddHostedService<OrderTimeoutBackgroundService>();
        }

        return services;
    }
}
