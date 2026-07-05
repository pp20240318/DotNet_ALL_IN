using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace WatchShop.Admin.Api.Extensions;

public static class AdminRateLimitExtensions
{
    public static IServiceCollection AddAdminRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddFixedWindowLimiter("admin-fixed", limiter =>
            {
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.PermitLimit = 200;
                limiter.QueueLimit = 0;
            });
        });
        return services;
    }

    public static WebApplication UseAdminRateLimiting(this WebApplication app)
    {
        app.UseRateLimiter();
        return app;
    }
}
