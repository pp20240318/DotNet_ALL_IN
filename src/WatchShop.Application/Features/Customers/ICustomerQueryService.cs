namespace WatchShop.Application.Features.Customers;

public class CustomerAdminResponse
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
}

public interface ICustomerQueryService
{
    Task<Application.Common.PagedResult<CustomerAdminResponse>> GetPagedAsync(
        int page,
        int pageSize,
        string? keyword = null,
        CancellationToken cancellationToken = default);
}
