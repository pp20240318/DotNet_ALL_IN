using SqlSugar;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Common;
using WatchShop.Application.Exceptions;
using WatchShop.Domain.Entities;
using WatchShop.Infrastructure.Security;

namespace WatchShop.Infrastructure.Services;

public class AdminManagementService : IAdminManagementService
{
    private readonly ISqlSugarClient _db;
    private readonly IRbacService _rbacService;

    public AdminManagementService(ISqlSugarClient db, IRbacService rbacService)
    {
        _db = db;
        _rbacService = rbacService;
    }

    public async Task<long> CreateAsync(CreateAdminRequest request, CancellationToken cancellationToken = default)
    {
        if (await _db.Queryable<Admin>().AnyAsync(x => x.Username == request.Username))
        {
            throw new BusinessException("用户名已存在");
        }

        var admin = new Admin
        {
            Username = request.Username.Trim(),
            PasswordHash = PasswordHasher.Hash(request.Password),
            DisplayName = request.DisplayName.Trim(),
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        };
        admin.Id = await _db.Insertable(admin).ExecuteReturnIdentityAsync();
        await _rbacService.SetAdminRolesAsync(admin.Id, request.Roles, cancellationToken);
        return admin.Id;
    }

    public async Task UpdateAsync(long adminId, UpdateAdminRequest request, CancellationToken cancellationToken = default)
    {
        var admin = await _db.Queryable<Admin>().FirstAsync(x => x.Id == adminId);
        if (admin is null)
        {
            throw new BusinessException(ApiResultCode.NotFound, "管理员不存在");
        }

        admin.DisplayName = request.DisplayName.Trim();
        admin.IsEnabled = request.IsEnabled;
        await _db.Updateable(admin).ExecuteCommandAsync();
    }
}
