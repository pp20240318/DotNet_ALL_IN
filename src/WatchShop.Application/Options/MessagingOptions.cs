namespace WatchShop.Application.Options;

public class MessagingOptions
{
    public const string SectionName = "Messaging";

    public bool UseCap { get; set; }
    public string RabbitMqHost { get; set; } = "localhost";
    public int RabbitMqPort { get; set; } = 5672;
    public string RabbitMqUser { get; set; } = "guest";
    public string RabbitMqPassword { get; set; } = "guest";
    public string DefaultGroup { get; set; } = "watchshop";
}
