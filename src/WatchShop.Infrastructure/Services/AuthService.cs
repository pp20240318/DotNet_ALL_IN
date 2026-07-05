using SqlSugar;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Common;
using WatchShop.Application.Dtos.Auth;
using WatchShop.Application.Exceptions;
using WatchShop.Domain.Entities;
using WatchShop.Infrastructure.Security;

namespace WatchShop.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ISqlSugarClient _db;
    private readonly JwtTokenService _jwtTokenService;

    public AuthService(ISqlSugarClient db, JwtTokenService jwtTokenService)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var admin = await _db.Queryable<Admin>()
            .FirstAsync(x => x.Username == request.Username);

        if (admin is null || !admin.IsEnabled || !PasswordHasher.Verify(request.Password, admin.PasswordHash))
        {
            throw new BusinessException(ApiResultCode.Unauthorized, "用户名或密码错误");
        }

        var (token, expiresAt) = _jwtTokenService.CreateToken(admin);

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            Username = admin.Username,
            DisplayName = admin.DisplayName
        };
    }

    public async Task<AdminProfileResponse> GetProfileAsync(long adminId, CancellationToken cancellationToken = default)
    {
        var admin = await _db.Queryable<Admin>()
            .FirstAsync(x => x.Id == adminId);

        if (admin is null || !admin.IsEnabled)
        {
            throw new BusinessException(ApiResultCode.NotFound, "管理员不存在或已禁用");
        }

        return new AdminProfileResponse
        {
            Id = admin.Id,
            Username = admin.Username,
            DisplayName = admin.DisplayName
        };
    }
}
