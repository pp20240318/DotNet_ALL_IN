using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WatchShop.Application.Features.Catalog;
using WatchShop.Application.Features.Catalog.Dtos;
using WatchShop.Application.Features.Store;
using WatchShop.Application.Features.Store.Dtos;

namespace WatchShop.Store.Api.Controllers;

[Route("catalog")]
[EnableRateLimiting("store-fixed")]
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
[EnableRateLimiting("store-fixed")]
public class StoreOrderController : ApiControllerBase
{
    private readonly ICatalogService _catalogService;
    private readonly IStoreOrderService _storeOrderService;

    public StoreOrderController(ICatalogService catalogService, IStoreOrderService storeOrderService)
    {
        _catalogService = catalogService;
        _storeOrderService = storeOrderService;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] StoreOrderCreateRequest request)
    {
        var customerId = GetCustomerIdOrNull();
        return Success(
            new { id = await _catalogService.CreateStoreOrderAsync(request, customerId) },
            "下单成功");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetMyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Success(await _storeOrderService.GetMyOrdersAsync(GetCustomerId(), page, pageSize));

    [HttpGet("{id:long}")]
    [Authorize]
    public async Task<IActionResult> GetMyOrderDetail(long id)
    {
        var item = await _storeOrderService.GetMyOrderDetailAsync(GetCustomerId(), id);
        return item is null ? Fail(404, "订单不存在") : Success(item);
    }

    [HttpPost("{id:long}/pay")]
    [Authorize]
    public async Task<IActionResult> Pay(long id)
    {
        await _storeOrderService.PayOrderAsync(GetCustomerId(), id);
        return Success(true, "支付成功（模拟）");
    }

    [HttpPost("{id:long}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(long id)
    {
        await _storeOrderService.CancelOrderAsync(GetCustomerId(), id);
        return Success(true, "取消成功");
    }

    private long? GetCustomerIdOrNull()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(claim, out var id) ? id : null;
    }

    private long GetCustomerId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("未登录");
        return long.Parse(claim);
    }
}

[Route("store/auth")]
[EnableRateLimiting("store-fixed")]
public class StoreAuthController : ApiControllerBase
{
    private readonly IStoreAuthService _storeAuthService;

    public StoreAuthController(IStoreAuthService storeAuthService) => _storeAuthService = storeAuthService;

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] CustomerRegisterRequest request)
        => Success(await _storeAuthService.RegisterAsync(request), "注册成功");

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] CustomerLoginRequest request)
        => Success(await _storeAuthService.LoginAsync(request), "登录成功");

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
        => Success(await _storeAuthService.GetProfileAsync(GetCustomerId()));

    private long GetCustomerId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("未登录");
        return long.Parse(claim);
    }
}

[Route("store/cart")]
[Authorize]
[EnableRateLimiting("store-fixed")]
public class CartController : ApiControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService) => _cartService = cartService;

    [HttpGet]
    public async Task<IActionResult> GetCart()
        => Success(await _cartService.GetCartAsync(GetCustomerId()));

    [HttpPost]
    public async Task<IActionResult> AddItem([FromBody] CartAddRequest request)
    {
        await _cartService.AddItemAsync(GetCustomerId(), request);
        return Success(true, "已加入购物车");
    }

    [HttpPut("{skuId:long}")]
    public async Task<IActionResult> UpdateQuantity(long skuId, [FromBody] CartUpdateRequest request)
    {
        await _cartService.UpdateQuantityAsync(GetCustomerId(), skuId, request.Quantity);
        return Success(true, "已更新");
    }

    [HttpDelete("{skuId:long}")]
    public async Task<IActionResult> RemoveItem(long skuId)
    {
        await _cartService.RemoveItemAsync(GetCustomerId(), skuId);
        return Success(true, "已移除");
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CartCheckoutRequest request)
        => Success(new { id = await _cartService.CheckoutAsync(GetCustomerId(), request) }, "结算成功");

    private long GetCustomerId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("未登录");
        return long.Parse(claim);
    }
}
