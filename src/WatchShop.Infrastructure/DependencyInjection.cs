using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SqlSugar;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Options;
using WatchShop.Infrastructure.Services;

namespace WatchShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Singleton：SqlSugarScope 线程安全，全局一个实例
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

        // Scoped：每个 HTTP 请求一个实例（常规业务 Service 用这个）
        services.AddScoped<IDatabaseHealthService, DatabaseHealthService>();

        return services;
    }
}
