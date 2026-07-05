using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchShop.Admin.Api.Filters;
using WatchShop.Application.Abstractions;

namespace WatchShop.Admin.Api.Controllers;

[Route("categories")]
[Authorize]
[OperationLogFilter]
public class CategoryController : ApiControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Success(await _categoryService.GetPagedAsync(page, pageSize));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _categoryService.GetByIdAsync(id);
        return item is null ? Fail(404, "分类不存在") : Success(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryCreateRequest request)
        => Success(new { id = await _categoryService.CreateAsync(request) }, "创建成功");

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] CategoryUpdateRequest request)
    {
        await _categoryService.UpdateAsync(id, request);
        return Success(true, "更新成功");
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _categoryService.DeleteAsync(id);
        return Success(true, "删除成功");
    }
}
