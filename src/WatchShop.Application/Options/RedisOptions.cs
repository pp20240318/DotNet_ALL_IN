namespace WatchShop.Application.Options;

public class RedisOptions
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = "localhost:6379";

    public bool Enabled { get; set; } = true;

    public int DefaultExpirationMinutes { get; set; } = 30;
}
