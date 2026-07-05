using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchShop.Admin.Api.Filters;
using WatchShop.Application.Abstractions;

namespace WatchShop.Admin.Api.Controllers;

[Route("orders")]
[Authorize]
[OperationLogFilter]
public class OrderController : ApiControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] OrderQueryRequest query)
        => Success(await _orderService.GetPagedAsync(query));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _orderService.GetByIdAsync(id);
        return item is null ? Fail(404, "订单不存在") : Success(item);
    }

    [HttpPost("demo")]
    public async Task<IActionResult> CreateDemo()
        => Success(new { id = await _orderService.CreateDemoOrderAsync() }, "演示订单创建成功");

    [HttpPost("{id:long}/ship")]
    public async Task<IActionResult> Ship(long id)
    {
        await _orderService.ShipAsync(id);
        return Success(true, "发货成功");
    }

    [HttpPost("{id:long}/cancel")]
    public async Task<IActionResult> Cancel(long id)
    {
        await _orderService.CancelAsync(id);
        return Success(true, "取消成功");
    }
}
