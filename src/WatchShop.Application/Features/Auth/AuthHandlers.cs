using MediatR;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Dtos.Auth;

namespace WatchShop.Application.Features.Auth;

public record AdminLoginCommand(LoginRequest Request) : IRequest<LoginResponse>;
public record AdminRefreshCommand(RefreshTokenRequest Request) : IRequest<LoginResponse>;
public record GetAdminProfileQuery(long AdminId) : IRequest<AdminProfileResponse>;

public class AdminLoginCommandHandler(IAuthService service)
    : IRequestHandler<AdminLoginCommand, LoginResponse>
{
    public Task<LoginResponse> Handle(AdminLoginCommand request, CancellationToken cancellationToken)
        => service.LoginAsync(request.Request, cancellationToken);
}

public class AdminRefreshCommandHandler(IAuthService service)
    : IRequestHandler<AdminRefreshCommand, LoginResponse>
{
    public Task<LoginResponse> Handle(AdminRefreshCommand request, CancellationToken cancellationToken)
        => service.RefreshAsync(request.Request, cancellationToken);
}

public class GetAdminProfileQueryHandler(IAuthService service)
    : IRequestHandler<GetAdminProfileQuery, AdminProfileResponse>
{
    public Task<AdminProfileResponse> Handle(GetAdminProfileQuery request, CancellationToken cancellationToken)
        => service.GetProfileAsync(request.AdminId, cancellationToken);
}
