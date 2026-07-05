using WatchShop.Application.Common;

namespace WatchShop.Application.Features.Notifications;

public interface INotificationService
{
    Task<PagedResult<NotificationResponse>> GetPagedAsync(int page, int pageSize, bool? unreadOnly = null, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(long id, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default);
}

public class NotificationResponse
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public long? RelatedId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
