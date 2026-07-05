using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchShop.Admin.Api.Filters;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Features.ProductSkus;

namespace WatchShop.Admin.Api.Controllers;

[Route("skus")]
[Authorize]
[OperationLogFilter]
public class ProductSkuController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public ProductSkuController(IMediator mediator) => _mediator = mediator;

    [HttpGet("by-product/{productId:long}")]
    public async Task<IActionResult> GetByProductId(long productId)
        => Success(await _mediator.Send(new GetSkusByProductQuery(productId)));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _mediator.Send(new GetSkuByIdQuery(id));
        return item is null ? Fail(404, "SKU 不存在") : Success(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductSkuCreateRequest request)
        => Success(new { id = await _mediator.Send(new CreateSkuCommand(request)) }, "创建成功");

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] ProductSkuUpdateRequest request)
    {
        await _mediator.Send(new UpdateSkuCommand(id, request));
        return Success(true, "更新成功");
    }

    [HttpPost("{id:long}/stock-in")]
    public async Task<IActionResult> StockIn(long id, [FromQuery] int quantity)
    {
        await _mediator.Send(new StockInCommand(id, quantity));
        return Success(true, "入库成功");
    }

    [HttpPost("{id:long}/stock-out")]
    public async Task<IActionResult> StockOut(long id, [FromQuery] int quantity)
    {
        await _mediator.Send(new StockOutCommand(id, quantity));
        return Success(true, "出库成功");
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _mediator.Send(new DeleteSkuCommand(id));
        return Success(true, "删除成功");
    }
}
