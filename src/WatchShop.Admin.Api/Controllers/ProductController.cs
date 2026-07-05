using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchShop.Admin.Api.Filters;
using WatchShop.Application.Abstractions;
using WatchShop.Domain.Enums;

namespace WatchShop.Admin.Api.Controllers;

[Route("products")]
[Authorize]
[OperationLogFilter]
public class ProductController : ApiControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] ProductQueryRequest query)
        => Success(await _productService.GetPagedAsync(query));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _productService.GetByIdAsync(id);
        return item is null ? Fail(404, "商品不存在") : Success(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductCreateRequest request)
        => Success(new { id = await _productService.CreateAsync(request) }, "创建成功");

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] ProductUpdateRequest request)
    {
        await _productService.UpdateAsync(id, request);
        return Success(true, "更新成功");
    }

    [HttpPut("{id:long}/status")]
    public async Task<IActionResult> ChangeStatus(long id, [FromQuery] ProductStatus status)
    {
        await _productService.ChangeStatusAsync(id, status);
        return Success(true, "状态更新成功");
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _productService.DeleteAsync(id);
        return Success(true, "删除成功");
    }
}
