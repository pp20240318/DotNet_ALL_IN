using SqlSugar;

namespace WatchShop.Domain.Entities;

[SugarTable("role")]
public class Role
{
    [SugarColumn(IsPrimaryKey = true, Length = 50, IsNullable = false)]
    public string Code { get; set; } = string.Empty;

    [SugarColumn(Length = 100, IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    [SugarColumn(Length = 2000, IsNullable = false)]
    public string PermissionsJson { get; set; } = "[]";
}

[SugarTable("admin_role")]
public class AdminRole
{
    [SugarColumn(IsPrimaryKey = true)]
    public long AdminId { get; set; }

    [SugarColumn(IsPrimaryKey = true, Length = 50)]
    public string RoleCode { get; set; } = string.Empty;
}
