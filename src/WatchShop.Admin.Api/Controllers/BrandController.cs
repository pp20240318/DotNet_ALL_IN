using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchShop.Admin.Api.Filters;
using WatchShop.Application.Abstractions;

namespace WatchShop.Admin.Api.Controllers;

[Route("brands")]
[Authorize]
[OperationLogFilter]
public class BrandController : ApiControllerBase
{
    private readonly IBrandService _brandService;

    public BrandController(IBrandService brandService)
    {
        _brandService = brandService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Success(await _brandService.GetPagedAsync(page, pageSize));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _brandService.GetByIdAsync(id);
        return item is null ? Fail(404, "品牌不存在") : Success(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BrandCreateRequest request)
        => Success(new { id = await _brandService.CreateAsync(request) }, "创建成功");

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] BrandUpdateRequest request)
    {
        await _brandService.UpdateAsync(id, request);
        return Success(true, "更新成功");
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _brandService.DeleteAsync(id);
        return Success(true, "删除成功");
    }
}
