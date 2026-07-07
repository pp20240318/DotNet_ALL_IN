using System.Text.Json;
using SqlSugar;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Authorization;
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
}
