namespace WatchShop.Application.Authorization;

public static class AppPermissions
{
    public const string ProductRead = "product:read";
    public const string ProductWrite = "product:write";
    public const string OrderRead = "order:read";
    public const string OrderWrite = "order:write";
    public const string BrandWrite = "brand:write";
    public const string DashboardRead = "dashboard:read";
    public const string SystemAdmin = "system:admin";

    public static readonly string[] All =
    [
        ProductRead, ProductWrite, OrderRead, OrderWrite,
        BrandWrite, DashboardRead, SystemAdmin
    ];

    public static readonly IReadOnlyDictionary<string, string[]> RolePermissions = new Dictionary<string, string[]>
    {
        ["SuperAdmin"] = All,
        ["Operator"] = [ProductRead, ProductWrite, OrderRead, OrderWrite, BrandWrite, DashboardRead],
        ["Viewer"] = [ProductRead, OrderRead, DashboardRead]
    };
}

public static class AppRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Operator = "Operator";
    public const string Viewer = "Viewer";
}
