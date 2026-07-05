using WatchShop.Application.Common;
using WatchShop.Application.Features.Store.Dtos;

namespace WatchShop.Application.Features.Store;

public interface IStoreOrderService
{
    Task<PagedResult<StoreOrderSummaryResponse>> GetMyOrdersAsync(long customerId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<StoreOrderDetailResponse?> GetMyOrderDetailAsync(long customerId, long orderId, CancellationToken cancellationToken = default);
    Task PayOrderAsync(long customerId, long orderId, CancellationToken cancellationToken = default);
    Task CancelOrderAsync(long customerId, long orderId, CancellationToken cancellationToken = default);
}
