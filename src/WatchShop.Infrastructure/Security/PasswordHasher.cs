namespace WatchShop.Infrastructure.Security;

/// <summary>
/// 密码哈希（BCrypt 加盐）
/// </summary>
public static class PasswordHasher
{
    public static string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public static bool Verify(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
