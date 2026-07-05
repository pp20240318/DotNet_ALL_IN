namespace WatchShop.Application.Options;

public class OrderOptions
{
    public const string SectionName = "Order";

    public int PaymentTimeoutMinutes { get; set; } = 30;
}
