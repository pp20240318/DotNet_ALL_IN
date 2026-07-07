namespace WatchShop.Application.Options;

public class PaymentOptions
{
    public const string SectionName = "Payment";

    public string WebhookSecret { get; set; } = "dev-webhook-secret";
}
