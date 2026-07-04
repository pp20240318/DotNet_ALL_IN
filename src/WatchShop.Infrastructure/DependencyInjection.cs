using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using WatchShop.Application.Abstractions;
using WatchShop.Infrastructure.Services;

namespace WatchShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' not found.");
        // Singleton：SqlSugarScope 线程安全，全局一个实例
        services.AddSingleton<ISqlSugarClient>(_ =>
        {
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