using Hangfire.Dashboard;
using WatchShop.Application.Authorization;

namespace WatchShop.Admin.Api.Extensions;

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();
        return http.User.Identity?.IsAuthenticated == true
            && (http.User.IsInRole(AppRoles.SuperAdmin)
                || http.User.HasClaim("permission", AppPermissions.SystemAdmin));
    }
}
