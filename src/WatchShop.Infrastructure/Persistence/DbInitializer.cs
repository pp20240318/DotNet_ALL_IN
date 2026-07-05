using SqlSugar;
using WatchShop.Domain.Entities;
using WatchShop.Infrastructure.Security;

namespace WatchShop.Infrastructure.Persistence;

/// <summary>
/// 数据库初始化：Code First 建表 + 种子数据
/// </summary>
public class DbInitializer
{
    private readonly ISqlSugarClient _db;

    public DbInitializer(ISqlSugarClient db)
    {
        _db = db;
    }

    public void Initialize()
    {
        _db.CodeFirst.InitTables(typeof(Admin));
        SeedAdminUser();
    }

    private void SeedAdminUser()
    {
        const string defaultUsername = "admin";
        var exists = _db.Queryable<Admin>().Any(x => x.Username == defaultUsername);
        if (exists)
        {
            return;
        }

        _db.Insertable(new Admin
        {
            Username = defaultUsername,
            PasswordHash = PasswordHasher.Hash("Admin@123"),
            DisplayName = "系统管理员",
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        }).ExecuteCommand();
    }
}
