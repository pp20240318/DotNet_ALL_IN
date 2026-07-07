using Microsoft.AspNetCore.Authorization;

namespace WatchShop.Admin.Api.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission) => Permission = permission;
    public string Permission { get; }
}

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.IsInRole("SuperAdmin")
            || context.User.HasClaim("permission", requirement.Permission)
            || context.User.HasClaim("permission", Application.Authorization.AppPermissions.SystemAdmin))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission) => Policy = permission;
}
