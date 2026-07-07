using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using SqlSugar;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Common;
using WatchShop.Application.Dtos.Auth;
using WatchShop.Application.Exceptions;
using WatchShop.Application.Options;
using WatchShop.Domain.Entities;
using WatchShop.Infrastructure.Security;

namespace WatchShop.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ISqlSugarClient _db;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IRbacService _rbacService;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        ISqlSugarClient db,
        JwtTokenService jwtTokenService,
        IRbacService rbacService,
        IOptions<JwtOptions> jwtOptions)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _rbacService = rbacService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var admin = await _db.Queryable<Admin>()
            .FirstAsync(x => x.Username == request.Username);

        if (admin is null || !admin.IsEnabled || !PasswordHasher.Verify(request.Password, admin.PasswordHash))
        {
            throw new BusinessException(ApiResultCode.Unauthorized, "用户名或密码错误");
        }

        return await IssueTokensAsync(admin, cancellationToken);
    }

    public async Task<LoginResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var stored = await _db.Queryable<AdminRefreshToken>()
            .FirstAsync(x => x.Token == request.RefreshToken && !x.IsRevoked);

        if (stored is null || stored.ExpiresAt < DateTime.UtcNow)
        {
            throw new BusinessException(ApiResultCode.Unauthorized, "Refresh Token 无效或已过期");
        }

        var admin = await _db.Queryable<Admin>().FirstAsync(x => x.Id == stored.AdminId);
        if (admin is null || !admin.IsEnabled)
        {
            throw new BusinessException(ApiResultCode.Unauthorized, "管理员不存在或已禁用");
        }

        stored.IsRevoked = true;
        await _db.Updateable(stored).ExecuteCommandAsync();

        return await IssueTokensAsync(admin, cancellationToken);
    }

    public async Task<AdminProfileResponse> GetProfileAsync(long adminId, CancellationToken cancellationToken = default)
    {
        var admin = await _db.Queryable<Admin>()
            .FirstAsync(x => x.Id == adminId);

        if (admin is null || !admin.IsEnabled)
        {
            throw new BusinessException(ApiResultCode.NotFound, "管理员不存在或已禁用");
        }

        var roles = await _rbacService.GetRoleCodesAsync(adminId, cancellationToken);
        var permissions = await _rbacService.GetPermissionsAsync(adminId, cancellationToken);

        return new AdminProfileResponse
        {
            Id = admin.Id,
            Username = admin.Username,
            DisplayName = admin.DisplayName,
            Roles = roles.ToList(),
            Permissions = permissions.ToList()
        };
    }

    private async Task<LoginResponse> IssueTokensAsync(Admin admin, CancellationToken cancellationToken)
    {
        var roles = await _rbacService.GetRoleCodesAsync(admin.Id, cancellationToken);
        var permissions = await _rbacService.GetPermissionsAsync(admin.Id, cancellationToken);
        var (token, expiresAt) = _jwtTokenService.CreateToken(admin, roles, permissions);

        var refreshToken = GenerateRefreshToken();
        var refreshExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshExpirationDays);

        await _db.Insertable(new AdminRefreshToken
        {
            Id = SnowFlakeSingle.Instance.NextId(),
            AdminId = admin.Id,
            Token = refreshToken,
            ExpiresAt = refreshExpiresAt,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        }).ExecuteCommandAsync();

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            RefreshToken = refreshToken,
            RefreshExpiresAt = refreshExpiresAt,
            Username = admin.Username,
            DisplayName = admin.DisplayName,
            Roles = roles.ToList(),
            Permissions = permissions.ToList()
        };
    }

    private static string GenerateRefreshToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
