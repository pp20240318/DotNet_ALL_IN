using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchShop.Admin.Api.Filters;
using WatchShop.Application.Features.Categories;

namespace WatchShop.Admin.Api.Controllers;

[Route("categories")]
[Authorize]
[OperationLogFilter]
public class CategoryController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CategoryController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Success(await _mediator.Send(new GetCategoriesPagedQuery(page, pageSize)));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _mediator.Send(new GetCategoryByIdQuery(id));
        return item is null ? Fail(404, "分类不存在") : Success(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Application.Abstractions.CategoryCreateRequest request)
        => Success(new { id = await _mediator.Send(new CreateCategoryCommand(request)) }, "创建成功");

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] Application.Abstractions.CategoryUpdateRequest request)
    {
        await _mediator.Send(new UpdateCategoryCommand(id, request));
        return Success(true, "更新成功");
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _mediator.Send(new DeleteCategoryCommand(id));
        return Success(true, "删除成功");
    }
}
