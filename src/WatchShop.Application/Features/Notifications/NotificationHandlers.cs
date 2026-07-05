using MediatR;
using WatchShop.Application.Common;
using WatchShop.Application.Features.Notifications;

namespace WatchShop.Application.Features.Notifications;

public record GetNotificationsPagedQuery(int Page = 1, int PageSize = 20, bool? UnreadOnly = null)
    : IRequest<PagedResult<NotificationResponse>>;

public record GetUnreadNotificationCountQuery() : IRequest<int>;

public record MarkNotificationReadCommand(long Id) : IRequest;

public record MarkAllNotificationsReadCommand() : IRequest;

public class GetNotificationsPagedQueryHandler(INotificationService service)
    : IRequestHandler<GetNotificationsPagedQuery, PagedResult<NotificationResponse>>
{
    public Task<PagedResult<NotificationResponse>> Handle(GetNotificationsPagedQuery request, CancellationToken cancellationToken)
        => service.GetPagedAsync(request.Page, request.PageSize, request.UnreadOnly, cancellationToken);
}

public class GetUnreadNotificationCountQueryHandler(INotificationService service)
    : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    public Task<int> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
        => service.GetUnreadCountAsync(cancellationToken);
}

public class MarkNotificationReadCommandHandler(INotificationService service)
    : IRequestHandler<MarkNotificationReadCommand>
{
    public Task Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
        => service.MarkAsReadAsync(request.Id, cancellationToken);
}

public class MarkAllNotificationsReadCommandHandler(INotificationService service)
    : IRequestHandler<MarkAllNotificationsReadCommand>
{
    public Task Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
        => service.MarkAllAsReadAsync(cancellationToken);
}
