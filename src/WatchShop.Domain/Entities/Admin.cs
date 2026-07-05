using SqlSugar;

namespace WatchShop.Domain.Entities;

/// <summary>
/// 管理员实体
/// </summary>
[SugarTable("admin")]
public class Admin
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    [SugarColumn(Length = 50, IsNullable = false)]
    public string Username { get; set; } = string.Empty;

    [SugarColumn(Length = 200, IsNullable = false)]
    public string PasswordHash { get; set; } = string.Empty;

    [SugarColumn(Length = 50, IsNullable = false)]
    public string DisplayName { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
