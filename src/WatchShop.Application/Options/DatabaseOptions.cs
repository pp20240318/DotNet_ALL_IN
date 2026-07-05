namespace WatchShop.Application.Options;

/// <summary>
/// 数据库连接配置（绑定 appsettings.json → Database 节点）
/// </summary>
public class DatabaseOptions
{
    public const string SectionName = "Database";

    public string Server { get; set; } = "localhost";
    public int Port { get; set; } = 3306;
    public string Database { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public string BuildConnectionString()
    {
        return $"Server={Server};Port={Port};Database={Database};User={User};Password={Password};SslMode=None;";
    }
}
