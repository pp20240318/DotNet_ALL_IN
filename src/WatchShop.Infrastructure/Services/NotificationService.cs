using SqlSugar;
using WatchShop.Application.Common;
using WatchShop.Application.Features.Notifications;
using WatchShop.Domain.Entities;

namespace WatchShop.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ISqlSugarClient _db;

    public NotificationService(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<PagedResult<NotificationResponse>> GetPagedAsync(
        int page, int pageSize, bool? unreadOnly = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Queryable<Notification>().Where(x => !x.IsDeleted);
        if (unreadOnly == true)
        {
            query = query.Where(x => !x.IsRead);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<NotificationResponse>
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items.Select(Map).ToList()
        };
    }

    public async Task MarkAsReadAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Queryable<Notification>().InSingleAsync(id)
            ?? throw new Application.Exceptions.BusinessException("通知不存在");

        entity.IsRead = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.Updateable(entity).UpdateColumns(x => new { x.IsRead, x.UpdatedAt }).ExecuteCommandAsync();
    }

    public async Task MarkAllAsReadAsync(CancellationToken cancellationToken = default)
    {
        await _db.Updateable<Notification>()
            .SetColumns(x => new Notification { IsRead = true, UpdatedAt = DateTime.UtcNow })
            .Where(x => !x.IsDeleted && !x.IsRead)
            .ExecuteCommandAsync();
    }

    public Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default)
        => _db.Queryable<Notification>().Where(x => !x.IsDeleted && !x.IsRead).CountAsync();

    private static NotificationResponse Map(Notification entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        Content = entity.Content,
        Category = entity.Category,
        RelatedId = entity.RelatedId,
        IsRead = entity.IsRead,
        CreatedAt = entity.CreatedAt
    };
}
