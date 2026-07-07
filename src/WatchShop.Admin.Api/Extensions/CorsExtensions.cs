namespace WatchShop.Admin.Api.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddAdminCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AdminOrigins").Get<string[]>()
            ??
            [
                "http://localhost:5173",
                "http://localhost:5174",
                "http://localhost:4173"
            ];

        services.AddCors(options =>
        {
            options.AddPolicy("admin-app", policy =>
                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });

        return services;
    }

    public static WebApplication UseAdminCors(this WebApplication app)
    {
        app.UseCors("admin-app");
        return app;
    }
}
