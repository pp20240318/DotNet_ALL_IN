using WatchShop.Application.Features.Store.Dtos;

namespace WatchShop.Application.Features.Store;

public interface IStoreAuthService
{
    Task<CustomerLoginResponse> RegisterAsync(CustomerRegisterRequest request, CancellationToken cancellationToken = default);
    Task<CustomerLoginResponse> LoginAsync(CustomerLoginRequest request, CancellationToken cancellationToken = default);
    Task<CustomerProfileResponse> GetProfileAsync(long customerId, CancellationToken cancellationToken = default);
}
