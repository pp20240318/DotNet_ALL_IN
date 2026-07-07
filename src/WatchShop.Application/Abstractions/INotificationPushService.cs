namespace WatchShop.Application.Abstractions;

public class NotificationPushMessage
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public long? RelatedId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public interface INotificationPushService
{
    Task PushAsync(NotificationPushMessage message, CancellationToken cancellationToken = default);
}
