using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchShop.Admin.Api.Authorization;
using WatchShop.Admin.Api.Filters;
using WatchShop.Application.Authorization;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Features.Products;
using WatchShop.Application.Features.Products.Commands;
using WatchShop.Domain.Enums;

namespace WatchShop.Admin.Api.Controllers;

[Route("products")]
[Authorize]
[OperationLogFilter]
public class ProductController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public ProductController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequirePermission(AppPermissions.ProductRead)]
    public async Task<IActionResult> GetPaged([FromQuery] ProductQueryRequest query)
        => Success(await _mediator.Send(new GetProductsPagedQuery(query)));

    [HttpGet("{id:long}")]
    [RequirePermission(AppPermissions.ProductRead)]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _mediator.Send(new GetProductByIdQuery(id));
        return item is null ? Fail(404, "商品不存在") : Success(item);
    }

    [HttpPost]
    [RequirePermission(AppPermissions.ProductWrite)]
    public async Task<IActionResult> Create([FromBody] ProductCreateRequest request)
        => Success(new { id = await _mediator.Send(new CreateProductCommand(request)) }, "创建成功");

    [HttpPut("{id:long}")]
    [RequirePermission(AppPermissions.ProductWrite)]
    public async Task<IActionResult> Update(long id, [FromBody] ProductUpdateRequest request)
    {
        await _mediator.Send(new UpdateProductCommand(id, request));
        return Success(true, "更新成功");
    }

    [HttpPut("{id:long}/status")]
    [RequirePermission(AppPermissions.ProductWrite)]
    public async Task<IActionResult> ChangeStatus(long id, [FromQuery] ProductStatus status)
    {
        await _mediator.Send(new ChangeProductStatusCommand(id, status));
        return Success(true, "状态更新成功");
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(AppPermissions.ProductWrite)]
    public async Task<IActionResult> Delete(long id)
    {
        await _mediator.Send(new DeleteProductCommand(id));
        return Success(true, "删除成功");
    }

    [HttpPost("{id:long}/cover")]
    [RequirePermission(AppPermissions.ProductWrite)]
    public async Task<IActionResult> UploadCover(long id, IFormFile file)
    {
        if (file.Length == 0)
        {
            return Fail(400, "请选择文件");
        }

        await using var stream = file.OpenReadStream();
        var url = await _mediator.Send(new UploadProductCoverCommand(id, stream, file.FileName));
        return Success(new { url }, "封面上传成功");
    }
}
