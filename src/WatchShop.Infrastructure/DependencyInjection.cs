using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SqlSugar;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Contracts.Persistence;
using WatchShop.Application.Features.Catalog;
using WatchShop.Application.Features.Notifications;
using WatchShop.Application.Features.Dashboard;
using WatchShop.Application.Features.OperationLogs;
using WatchShop.Application.Features.Search;
using WatchShop.Application.Features.Store;
using WatchShop.Application.Options;
using WatchShop.Infrastructure.Background;
using WatchShop.Infrastructure.Caching;
using WatchShop.Infrastructure.Persistence;
using WatchShop.Infrastructure.Security;
using WatchShop.Infrastructure.Services;
using WatchShop.Infrastructure.Storage;

namespace WatchShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ISqlSugarClient>(sp =>
        {
            var dbOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var connectionString = dbOptions.BuildConnectionString();

            if (string.IsNullOrWhiteSpace(dbOptions.Database))
            {
                throw new InvalidOperationException("Database options are not configured.");
            }

            return new SqlSugarScope(new ConnectionConfig
            {
                ConnectionString = connectionString,
                DbType = DbType.MySql,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            },
            db =>
            {
                db.Aop.OnLogExecuting = (sql, pars) =>
                {
                    Console.WriteLine($"[SqlSugar] {sql}");
                };
            });
        });

        services.AddWatchShopMessaging(configuration);
        services.AddWatchShopBackgroundJobs(configuration);

        services.AddSingleton<JwtTokenService>();
        services.AddScoped<IRbacService, RbacService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<DbInitializer>();
        services.AddScoped<CacheService>();
        services.AddScoped<OperationLogService>();
        services.AddScoped<IDatabaseHealthService, DatabaseHealthService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductSkuService, ProductSkuService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IStoreAuthService, StoreAuthService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IStoreOrderService, StoreOrderService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IOperationLogQueryService, OperationLogQueryService>();
        services.AddScoped<ICatalogCacheInvalidator, CatalogCacheInvalidator>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddSingleton<INotificationPushService, NullNotificationPushService>();
        services.AddWatchShopSearch(configuration);
        services.AddWatchShopStorage(configuration);

        return services;
    }

    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>() ?? new RedisOptions();

        if (redisOptions.Enabled)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisOptions.ConnectionString;
                options.InstanceName = "WatchShop:";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        return services;
    }
}
