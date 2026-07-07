using Microsoft.AspNetCore.Authorization;
using WatchShop.Application.Authorization;

namespace WatchShop.Admin.Api.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddWatchShopAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, Authorization.PermissionAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            foreach (var permission in AppPermissions.All)
            {
                options.AddPolicy(permission, policy =>
                    policy.Requirements.Add(new Authorization.PermissionRequirement(permission)));
            }
        });

        return services;
    }
}
