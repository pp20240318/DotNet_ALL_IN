namespace WatchShop.Application.Options;

/// <summary>
/// JWT 配置（步骤 06 使用，本步先注册 Options）
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 120;
    public int RefreshExpirationDays { get; set; } = 7;
}
