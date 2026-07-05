using WatchShop.Application.Dtos.Auth;

namespace WatchShop.Application.Abstractions;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<AdminProfileResponse> GetProfileAsync(long adminId, CancellationToken cancellationToken = default);
}
