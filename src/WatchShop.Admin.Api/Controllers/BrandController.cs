using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchShop.Admin.Api.Authorization;
using WatchShop.Admin.Api.Filters;
using WatchShop.Application.Authorization;
using WatchShop.Application.Features.Brands.Commands;
using WatchShop.Application.Features.Brands.Dtos;
using WatchShop.Application.Features.Brands.Queries;

namespace WatchShop.Admin.Api.Controllers;

[Route("brands")]
[Authorize]
[OperationLogFilter]
public class BrandController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public BrandController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequirePermission(AppPermissions.ProductRead)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Success(await _mediator.Send(new GetBrandsPagedQuery(page, pageSize)));

    [HttpGet("{id:long}")]
    [RequirePermission(AppPermissions.ProductRead)]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _mediator.Send(new GetBrandByIdQuery(id));
        return item is null ? Fail(404, "品牌不存在") : Success(item);
    }

    [HttpPost]
    [RequirePermission(AppPermissions.BrandWrite)]
    public async Task<IActionResult> Create([FromBody] BrandCreateRequest request)
        => Success(new { id = await _mediator.Send(new CreateBrandCommand(request)) }, "创建成功");

    [HttpPut("{id:long}")]
    [RequirePermission(AppPermissions.BrandWrite)]
    public async Task<IActionResult> Update(long id, [FromBody] BrandUpdateRequest request)
    {
        await _mediator.Send(new UpdateBrandCommand(id, request));
        return Success(true, "更新成功");
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(AppPermissions.BrandWrite)]
    public async Task<IActionResult> Delete(long id)
    {
        await _mediator.Send(new DeleteBrandCommand(id));
        return Success(true, "删除成功");
    }
}
