using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WatchShop.Application.Features.Catalog.Dtos;
using WatchShop.Application.Features.Store.Commands;
using WatchShop.Application.Features.Store.Dtos;

namespace WatchShop.Store.Api.Controllers;

[Route("catalog")]
[EnableRateLimiting("store-fixed")]
public class CatalogController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CatalogController(IMediator mediator) => _mediator = mediator;

    [HttpGet("brands")]
    public async Task<IActionResult> GetBrands()
        => Success(await _mediator.Send(new GetCatalogBrandsQuery()));

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
        => Success(await _mediator.Send(new GetCatalogCategoriesQuery()));

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts()
        => Success(await _mediator.Send(new GetCatalogProductsQuery()));

    [HttpGet("products/{id:long}")]
    public async Task<IActionResult> GetProductDetail(long id)
    {
        var item = await _mediator.Send(new GetCatalogProductDetailQuery(id));
        return item is null ? Fail(404, "商品不存在或未上架") : Success(item);
    }
}

[Route("store/orders")]
[EnableRateLimiting("store-fixed")]
public class StoreOrderController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public StoreOrderController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] StoreOrderCreateRequest request)
        => Success(new { id = await _mediator.Send(new CreateStoreOrderCommand(request, GetCustomerIdOrNull())) }, "下单成功");

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetMyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        => Success(await _mediator.Send(new GetMyOrdersQuery(GetCustomerId(), page, pageSize)));

    [HttpGet("{id:long}")]
    [Authorize]
    public async Task<IActionResult> GetMyOrderDetail(long id)
    {
        var item = await _mediator.Send(new GetMyOrderDetailQuery(GetCustomerId(), id));
        return item is null ? Fail(404, "订单不存在") : Success(item);
    }

    [HttpPost("{id:long}/pay")]
    [Authorize]
    public async Task<IActionResult> Pay(long id)
    {
        await _mediator.Send(new PayStoreOrderCommand(GetCustomerId(), id));
        return Success(true, "支付成功（模拟）");
    }

    [HttpPost("{id:long}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(long id)
    {
        await _mediator.Send(new CancelStoreOrderCommand(GetCustomerId(), id));
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
    private readonly IMediator _mediator;

    public StoreAuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] CustomerRegisterRequest request)
        => Success(await _mediator.Send(new StoreRegisterCommand(request)), "注册成功");

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] CustomerLoginRequest request)
        => Success(await _mediator.Send(new StoreLoginCommand(request)), "登录成功");

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
        => Success(await _mediator.Send(new GetStoreProfileQuery(GetCustomerId())));

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
    private readonly IMediator _mediator;

    public CartController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetCart()
        => Success(await _mediator.Send(new GetCartQuery(GetCustomerId())));

    [HttpPost]
    public async Task<IActionResult> AddItem([FromBody] CartAddRequest request)
    {
        await _mediator.Send(new AddCartItemCommand(GetCustomerId(), request));
        return Success(true, "已加入购物车");
    }

    [HttpPut("{skuId:long}")]
    public async Task<IActionResult> UpdateQuantity(long skuId, [FromBody] CartUpdateRequest request)
    {
        await _mediator.Send(new UpdateCartItemCommand(GetCustomerId(), skuId, request.Quantity));
        return Success(true, "已更新");
    }

    [HttpDelete("{skuId:long}")]
    public async Task<IActionResult> RemoveItem(long skuId)
    {
        await _mediator.Send(new RemoveCartItemCommand(GetCustomerId(), skuId));
        return Success(true, "已移除");
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CartCheckoutRequest request)
        => Success(new { id = await _mediator.Send(new CheckoutCartCommand(GetCustomerId(), request)) }, "结算成功");

    private long GetCustomerId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("未登录");
        return long.Parse(claim);
    }
}

[Route("webhooks")]
[EnableRateLimiting("store-fixed")]
public class PaymentWebhookController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public PaymentWebhookController(IMediator mediator) => _mediator = mediator;

    [HttpPost("payment/mock")]
    [AllowAnonymous]
    public async Task<IActionResult> MockPayment([FromBody] MockPaymentWebhookRequest request)
    {
        await _mediator.Send(new MockPaymentWebhookCommand(request.OrderId, request.Secret));
        return Success(true, "支付回调处理成功");
    }
}

public class MockPaymentWebhookRequest
{
    public long OrderId { get; set; }
    public string Secret { get; set; } = string.Empty;
}
