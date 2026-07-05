using WatchShop.Application.Features.Store.Dtos;

namespace WatchShop.Application.Features.Store;

public interface ICartService
{
    Task<List<CartItemResponse>> GetCartAsync(long customerId, CancellationToken cancellationToken = default);
    Task AddItemAsync(long customerId, CartAddRequest request, CancellationToken cancellationToken = default);
    Task UpdateQuantityAsync(long customerId, long skuId, int quantity, CancellationToken cancellationToken = default);
    Task RemoveItemAsync(long customerId, long skuId, CancellationToken cancellationToken = default);
    Task<long> CheckoutAsync(long customerId, CartCheckoutRequest request, CancellationToken cancellationToken = default);
}
