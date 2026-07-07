using System.Text.Json;
using SqlSugar;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Authorization;
using WatchShop.Application.Common;
using WatchShop.Application.Exceptions;
using WatchShop.Application.Features.Rbac;
using WatchShop.Domain.Entities;

namespace WatchShop.Infrastructure.Security;

public class RbacService : IRbacService
{
    private readonly ISqlSugarClient _db;

    public RbacService(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<string>> GetRoleCodesAsync(long adminId, CancellationToken cancellationToken = default)
    {
        return await _db.Queryable<AdminRole>()
            .Where(x => x.AdminId == adminId)
            .Select(x => x.RoleCode)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<string>> GetPermissionsAsync(long adminId, CancellationToken cancellationToken = default)
    {
        var roleCodes = await GetRoleCodesAsync(adminId, cancellationToken);
        if (roleCodes.Count == 0)
        {
            return [];
        }

        var roles = await _db.Queryable<Role>()
            .Where(x => roleCodes.Contains(x.Code))
            .ToListAsync();

        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var role in roles)
        {
            var items = JsonSerializer.Deserialize<string[]>(role.PermissionsJson) ?? [];
            foreach (var item in items)
            {
                permissions.Add(item);
            }
        }

        return permissions.ToList();
    }

    public async Task<bool> HasPermissionAsync(long adminId, string permission, CancellationToken cancellationToken = default)
    {
        var permissions = await GetPermissionsAsync(adminId, cancellationToken);
        return permissions.Contains(permission, StringComparer.OrdinalIgnoreCase)
            || permissions.Contains(AppPermissions.SystemAdmin, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IReadOnlyList<RoleResponse>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _db.Queryable<Role>().OrderBy(x => x.Code).ToListAsync();
        return roles.Select(role => new RoleResponse
        {
            Code = role.Code,
            Name = role.Name,
            Permissions = JsonSerializer.Deserialize<string[]>(role.PermissionsJson) ?? []
        }).ToList();
    }

    public async Task<IReadOnlyList<AdminRoleResponse>> GetAllAdminsWithRolesAsync(CancellationToken cancellationToken = default)
    {
        var admins = await _db.Queryable<Admin>().OrderBy(x => x.Id).ToListAsync();
        var adminRoles = await _db.Queryable<AdminRole>().ToListAsync();
        var roleLookup = adminRoles
            .GroupBy(x => x.AdminId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.RoleCode).ToList());

        return admins.Select(admin => new AdminRoleResponse
        {
            Id = admin.Id,
            Username = admin.Username,
            DisplayName = admin.DisplayName,
            IsEnabled = admin.IsEnabled,
            Roles = roleLookup.TryGetValue(admin.Id, out var roles) ? roles : []
        }).ToList();
    }

    public async Task SetAdminRolesAsync(long adminId, IReadOnlyList<string> roles, CancellationToken cancellationToken = default)
    {
        var admin = await _db.Queryable<Admin>().FirstAsync(x => x.Id == adminId);
        if (admin is null)
        {
            throw new BusinessException(ApiResultCode.NotFound, "管理员不存在");
        }

        var distinctRoles = roles
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (distinctRoles.Count == 0)
        {
            throw new BusinessException("至少分配一个角色");
        }

        var validRoles = await _db.Queryable<Role>()
            .Where(x => distinctRoles.Contains(x.Code))
            .Select(x => x.Code)
            .ToListAsync();

        if (validRoles.Count != distinctRoles.Count)
        {
            throw new BusinessException("存在无效的角色编码");
        }

        await _db.Deleteable<AdminRole>().Where(x => x.AdminId == adminId).ExecuteCommandAsync();
        await _db.Insertable(distinctRoles.Select(roleCode => new AdminRole
        {
            AdminId = adminId,
            RoleCode = roleCode
        }).ToList()).ExecuteCommandAsync();
    }
}
