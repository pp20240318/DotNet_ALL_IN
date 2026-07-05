using SqlSugar;

namespace WatchShop.Domain.Entities;

[SugarTable("customer")]
public class Customer : BaseEntity
{
    [SugarColumn(Length = 50, IsNullable = false)]
    public string Username { get; set; } = string.Empty;

    [SugarColumn(Length = 200, IsNullable = false)]
    public string PasswordHash { get; set; } = string.Empty;

    [SugarColumn(Length = 50, IsNullable = true)]
    public string? Nickname { get; set; }

    [SugarColumn(Length = 20, IsNullable = true)]
    public string? Phone { get; set; }

    [SugarColumn(Length = 100, IsNullable = true)]
    public string? Email { get; set; }

    public bool IsEnabled { get; set; } = true;
}
