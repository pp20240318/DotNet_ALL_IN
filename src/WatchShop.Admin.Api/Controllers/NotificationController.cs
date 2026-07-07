using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchShop.Admin.Api.Authorization;
using WatchShop.Application.Authorization;
using WatchShop.Application.Features.Notifications;

namespace WatchShop.Admin.Api.Controllers;

[Route("notifications")]
[Authorize]
public class NotificationController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public NotificationController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequirePermission(AppPermissions.DashboardRead)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool? unreadOnly = null)
        => Success(await _mediator.Send(new GetNotificationsPagedQuery(page, pageSize, unreadOnly)));

    [HttpGet("unread-count")]
    [RequirePermission(AppPermissions.DashboardRead)]
    public async Task<IActionResult> GetUnreadCount()
        => Success(new { count = await _mediator.Send(new GetUnreadNotificationCountQuery()) });

    [HttpPost("{id:long}/read")]
    [RequirePermission(AppPermissions.DashboardRead)]
    public async Task<IActionResult> MarkRead(long id)
    {
        await _mediator.Send(new MarkNotificationReadCommand(id));
        return Success(true, "已标记已读");
    }

    [HttpPost("read-all")]
    [RequirePermission(AppPermissions.DashboardRead)]
    public async Task<IActionResult> MarkAllRead()
    {
        await _mediator.Send(new MarkAllNotificationsReadCommand());
        return Success(true, "全部已读");
    }
}
