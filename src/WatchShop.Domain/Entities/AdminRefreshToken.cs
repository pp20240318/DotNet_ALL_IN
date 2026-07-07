using SqlSugar;

namespace WatchShop.Domain.Entities;

[SugarTable("admin_refresh_token")]
public class AdminRefreshToken
{
    [SugarColumn(IsPrimaryKey = true)]
    public long Id { get; set; }

    public long AdminId { get; set; }

    [SugarColumn(Length = 200, IsNullable = false)]
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
