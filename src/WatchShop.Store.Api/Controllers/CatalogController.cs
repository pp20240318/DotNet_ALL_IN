using Microsoft.AspNetCore.Mvc;
using WatchShop.Application.Features.Catalog;
using WatchShop.Application.Features.Catalog.Dtos;

namespace WatchShop.Store.Api.Controllers;

[Route("catalog")]
public class CatalogController : ApiControllerBase
{
    private readonly ICatalogService _catalogService;

    public CatalogController(ICatalogService catalogService) => _catalogService = catalogService;

    [HttpGet("brands")]
    public async Task<IActionResult> GetBrands()
        => Success(await _catalogService.GetBrandsAsync());

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
        => Success(await _catalogService.GetCategoriesAsync());

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts()
        => Success(await _catalogService.GetOnSaleProductsAsync());

    [HttpGet("products/{id:long}")]
    public async Task<IActionResult> GetProductDetail(long id)
    {
        var item = await _catalogService.GetProductDetailAsync(id);
        return item is null ? Fail(404, "商品不存在或未上架") : Success(item);
    }
}

[Route("store/orders")]
public class StoreOrderController : ApiControllerBase
{
    private readonly ICatalogService _catalogService;

    public StoreOrderController(ICatalogService catalogService) => _catalogService = catalogService;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StoreOrderCreateRequest request)
        => Success(new { id = await _catalogService.CreateStoreOrderAsync(request) }, "下单成功");
}
