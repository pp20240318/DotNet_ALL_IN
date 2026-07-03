using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using Microsoft.Extensions.Configuration;

namespace WatchShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' not found.");

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
                // 开发阶段可开 SQL 日志，对应 Overview 里的 AOP 拦截
                db.Aop.OnLogExecuting = (sql, pars) =>
                {
                    Console.WriteLine($"[SqlSugar] {sql}");
                };
            });
        });

        return services;
    }
}
