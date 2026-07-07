namespace WatchShop.Application.Abstractions;

public class CreateAdminRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}

public class UpdateAdminRequest
{
    public string DisplayName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
}

public interface IAdminManagementService
{
    Task<long> CreateAsync(CreateAdminRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(long adminId, UpdateAdminRequest request, CancellationToken cancellationToken = default);
}
