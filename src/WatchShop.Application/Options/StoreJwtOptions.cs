namespace WatchShop.Application.Options;

public class StoreJwtOptions
{
    public const string SectionName = "StoreJwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 720;
}
