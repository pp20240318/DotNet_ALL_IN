using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchShop.Admin.Api.Authorization;
using WatchShop.Application.Authorization;
using WatchShop.Application.Features.AdminQueries;

namespace WatchShop.Admin.Api.Controllers;

[Route("dashboard")]
[Authorize]
public class DashboardController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet("stats")]
    [RequirePermission(AppPermissions.DashboardRead)]
    public async Task<IActionResult> GetStats()
        => Success(await _mediator.Send(new GetDashboardStatsQuery()));
}

[Route("operation-logs")]
[Authorize]
public class OperationLogController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public OperationLogController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? module = null)
        => Success(await _mediator.Send(new GetOperationLogsPagedQuery(page, pageSize, module)));
}

[Route("search")]
[Authorize]
public class SearchController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public SearchController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequirePermission(AppPermissions.DashboardRead)]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 20)
        => Success(await _mediator.Send(new Application.Features.Search.GlobalSearchQuery(q, limit)));
}
