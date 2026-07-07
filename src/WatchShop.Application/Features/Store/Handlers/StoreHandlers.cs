using MediatR;
using WatchShop.Application.Features.Catalog;
using WatchShop.Application.Features.Catalog.Dtos;
using WatchShop.Application.Features.Store.Commands;
using WatchShop.Application.Features.Store.Dtos;

namespace WatchShop.Application.Features.Store.Handlers;

public class StoreRegisterCommandHandler(IStoreAuthService service)
    : IRequestHandler<StoreRegisterCommand, CustomerLoginResponse>
{
    public Task<CustomerLoginResponse> Handle(StoreRegisterCommand request, CancellationToken cancellationToken)
        => service.RegisterAsync(request.Request, cancellationToken);
}

public class StoreLoginCommandHandler(IStoreAuthService service)
    : IRequestHandler<StoreLoginCommand, CustomerLoginResponse>
{
    public Task<CustomerLoginResponse> Handle(StoreLoginCommand request, CancellationToken cancellationToken)
        => service.LoginAsync(request.Request, cancellationToken);
}

public class GetStoreProfileQueryHandler(IStoreAuthService service)
    : IRequestHandler<GetStoreProfileQuery, CustomerProfileResponse>
{
    public Task<CustomerProfileResponse> Handle(GetStoreProfileQuery request, CancellationToken cancellationToken)
        => service.GetProfileAsync(request.CustomerId, cancellationToken);
}

public class GetCatalogBrandsQueryHandler(ICatalogService service)
    : IRequestHandler<GetCatalogBrandsQuery, List<CatalogBrandResponse>>
{
    public Task<List<CatalogBrandResponse>> Handle(GetCatalogBrandsQuery request, CancellationToken cancellationToken)
        => service.GetBrandsAsync(cancellationToken);
}

public class GetCatalogCategoriesQueryHandler(ICatalogService service)
    : IRequestHandler<GetCatalogCategoriesQuery, List<CatalogCategoryResponse>>
{
    public Task<List<CatalogCategoryResponse>> Handle(GetCatalogCategoriesQuery request, CancellationToken cancellationToken)
        => service.GetCategoriesAsync(cancellationToken);
}

public class GetCatalogProductsQueryHandler(ICatalogService service)
    : IRequestHandler<GetCatalogProductsQuery, List<CatalogProductResponse>>
{
    public Task<List<CatalogProductResponse>> Handle(GetCatalogProductsQuery request, CancellationToken cancellationToken)
        => service.GetOnSaleProductsAsync(cancellationToken);
}

public class GetCatalogProductDetailQueryHandler(ICatalogService service)
    : IRequestHandler<GetCatalogProductDetailQuery, CatalogProductDetailResponse?>
{
    public Task<CatalogProductDetailResponse?> Handle(GetCatalogProductDetailQuery request, CancellationToken cancellationToken)
        => service.GetProductDetailAsync(request.Id, cancellationToken);
}

public class CreateStoreOrderCommandHandler(ICatalogService service)
    : IRequestHandler<CreateStoreOrderCommand, long>
{
    public Task<long> Handle(CreateStoreOrderCommand request, CancellationToken cancellationToken)
        => service.CreateStoreOrderAsync(request.Request, request.CustomerId, cancellationToken);
}

public class GetMyOrdersQueryHandler(IStoreOrderService service)
    : IRequestHandler<GetMyOrdersQuery, Common.PagedResult<StoreOrderSummaryResponse>>
{
    public Task<Common.PagedResult<StoreOrderSummaryResponse>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
        => service.GetMyOrdersAsync(request.CustomerId, request.Page, request.PageSize, cancellationToken);
}

public class GetMyOrderDetailQueryHandler(IStoreOrderService service)
    : IRequestHandler<GetMyOrderDetailQuery, StoreOrderDetailResponse?>
{
    public Task<StoreOrderDetailResponse?> Handle(GetMyOrderDetailQuery request, CancellationToken cancellationToken)
        => service.GetMyOrderDetailAsync(request.CustomerId, request.OrderId, cancellationToken);
}

public class PayStoreOrderCommandHandler(IStoreOrderService service)
    : IRequestHandler<PayStoreOrderCommand>
{
    public Task Handle(PayStoreOrderCommand request, CancellationToken cancellationToken)
        => service.PayOrderAsync(request.CustomerId, request.OrderId, cancellationToken);
}

public class CancelStoreOrderCommandHandler(IStoreOrderService service)
    : IRequestHandler<CancelStoreOrderCommand>
{
    public Task Handle(CancelStoreOrderCommand request, CancellationToken cancellationToken)
        => service.CancelOrderAsync(request.CustomerId, request.OrderId, cancellationToken);
}

public class GetCartQueryHandler(ICartService service)
    : IRequestHandler<GetCartQuery, List<CartItemResponse>>
{
    public Task<List<CartItemResponse>> Handle(GetCartQuery request, CancellationToken cancellationToken)
        => service.GetCartAsync(request.CustomerId, cancellationToken);
}

public class AddCartItemCommandHandler(ICartService service)
    : IRequestHandler<AddCartItemCommand>
{
    public Task Handle(AddCartItemCommand request, CancellationToken cancellationToken)
        => service.AddItemAsync(request.CustomerId, request.Request, cancellationToken);
}

public class UpdateCartItemCommandHandler(ICartService service)
    : IRequestHandler<UpdateCartItemCommand>
{
    public Task Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
        => service.UpdateQuantityAsync(request.CustomerId, request.SkuId, request.Quantity, cancellationToken);
}

public class RemoveCartItemCommandHandler(ICartService service)
    : IRequestHandler<RemoveCartItemCommand>
{
    public Task Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
        => service.RemoveItemAsync(request.CustomerId, request.SkuId, cancellationToken);
}

public class CheckoutCartCommandHandler(ICartService service)
    : IRequestHandler<CheckoutCartCommand, long>
{
    public Task<long> Handle(CheckoutCartCommand request, CancellationToken cancellationToken)
        => service.CheckoutAsync(request.CustomerId, request.Request, cancellationToken);
}

public class MockPaymentWebhookCommandHandler(
    IStoreOrderService service,
    Microsoft.Extensions.Options.IOptions<WatchShop.Application.Options.PaymentOptions> options)
    : IRequestHandler<MockPaymentWebhookCommand>
{
    public Task Handle(MockPaymentWebhookCommand request, CancellationToken cancellationToken)
    {
        if (request.Secret != options.Value.WebhookSecret)
        {
            throw new WatchShop.Application.Exceptions.BusinessException(
                WatchShop.Application.Common.ApiResultCode.Forbidden, "Webhook 密钥无效");
        }

        return service.PayOrderByWebhookAsync(request.OrderId, cancellationToken);
    }
}
