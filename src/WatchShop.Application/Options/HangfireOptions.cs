namespace WatchShop.Application.Options;

public class HangfireOptions
{
    public const string SectionName = "Hangfire";

    public bool Enabled { get; set; } = true;
    public string DashboardPath { get; set; } = "/hangfire";
}
