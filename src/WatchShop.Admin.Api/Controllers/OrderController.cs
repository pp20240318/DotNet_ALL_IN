using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchShop.Admin.Api.Authorization;
using WatchShop.Admin.Api.Filters;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Authorization;
using WatchShop.Application.Features.Orders;

namespace WatchShop.Admin.Api.Controllers;

[Route("orders")]
[Authorize]
[OperationLogFilter]
public class OrderController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public OrderController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequirePermission(AppPermissions.OrderRead)]
    public async Task<IActionResult> GetPaged([FromQuery] OrderQueryRequest query)
        => Success(await _mediator.Send(new GetOrdersPagedQuery(query)));

    [HttpGet("{id:long}")]
    [RequirePermission(AppPermissions.OrderRead)]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _mediator.Send(new GetOrderByIdQuery(id));
        return item is null ? Fail(404, "订单不存在") : Success(item);
    }

    [HttpPost("demo")]
    [RequirePermission(AppPermissions.OrderWrite)]
    public async Task<IActionResult> CreateDemo()
        => Success(new { id = await _mediator.Send(new CreateDemoOrderCommand()) }, "演示订单创建成功");

    [HttpPost("{id:long}/ship")]
    [RequirePermission(AppPermissions.OrderWrite)]
    public async Task<IActionResult> Ship(long id)
    {
        await _mediator.Send(new ShipOrderCommand(id));
        return Success(true, "发货成功");
    }

    [HttpPost("{id:long}/cancel")]
    [RequirePermission(AppPermissions.OrderWrite)]
    public async Task<IActionResult> Cancel(long id)
    {
        await _mediator.Send(new CancelOrderCommand(id));
        return Success(true, "取消成功");
    }
}
