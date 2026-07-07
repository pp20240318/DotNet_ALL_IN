namespace WatchShop.Application.Abstractions;

public interface IRbacService
{
    Task<IReadOnlyList<string>> GetRoleCodesAsync(long adminId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetPermissionsAsync(long adminId, CancellationToken cancellationToken = default);
    Task<bool> HasPermissionAsync(long adminId, string permission, CancellationToken cancellationToken = default);
}
