using WatchShop.Application.Authorization;

namespace WatchShop.Tests;

public class RbacTests
{
    [Fact]
    public void SuperAdmin_Should_Have_All_Permissions()
    {
        var permissions = AppPermissions.RolePermissions[AppRoles.SuperAdmin];
        Assert.Equal(AppPermissions.All.Length, permissions.Length);
    }

    [Fact]
    public void Viewer_Should_Not_Have_Write_Permissions()
    {
        var permissions = AppPermissions.RolePermissions[AppRoles.Viewer];
        Assert.DoesNotContain(AppPermissions.ProductWrite, permissions);
        Assert.DoesNotContain(AppPermissions.BrandWrite, permissions);
    }
}
