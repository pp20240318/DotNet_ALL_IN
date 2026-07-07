using MediatR;
using WatchShop.Application.Features.Catalog;
using WatchShop.Application.Features.Catalog.Dtos;
using WatchShop.Application.Features.Store;
using WatchShop.Application.Features.Store.Dtos;

namespace WatchShop.Application.Features.Store.Commands;

public record StoreRegisterCommand(CustomerRegisterRequest Request) : IRequest<CustomerLoginResponse>;
public record StoreLoginCommand(CustomerLoginRequest Request) : IRequest<CustomerLoginResponse>;
public record GetStoreProfileQuery(long CustomerId) : IRequest<CustomerProfileResponse>;

public record GetCatalogBrandsQuery() : IRequest<List<CatalogBrandResponse>>;
public record GetCatalogCategoriesQuery() : IRequest<List<CatalogCategoryResponse>>;
public record GetCatalogProductsQuery() : IRequest<List<CatalogProductResponse>>;
public record GetCatalogProductDetailQuery(long Id) : IRequest<CatalogProductDetailResponse?>;

public record CreateStoreOrderCommand(StoreOrderCreateRequest Request, long? CustomerId) : IRequest<long>;
public record GetMyOrdersQuery(long CustomerId, int Page = 1, int PageSize = 10) : IRequest<Common.PagedResult<StoreOrderSummaryResponse>>;
public record GetMyOrderDetailQuery(long CustomerId, long OrderId) : IRequest<StoreOrderDetailResponse?>;
public record PayStoreOrderCommand(long CustomerId, long OrderId) : IRequest;
public record CancelStoreOrderCommand(long CustomerId, long OrderId) : IRequest;

public record GetCartQuery(long CustomerId) : IRequest<List<CartItemResponse>>;
public record AddCartItemCommand(long CustomerId, CartAddRequest Request) : IRequest;
public record UpdateCartItemCommand(long CustomerId, long SkuId, int Quantity) : IRequest;
public record RemoveCartItemCommand(long CustomerId, long SkuId) : IRequest;
public record CheckoutCartCommand(long CustomerId, CartCheckoutRequest Request) : IRequest<long>;

public record MockPaymentWebhookCommand(long OrderId, string Secret) : IRequest;
