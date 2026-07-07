using SqlSugar;
using WatchShop.Application.Common;
using WatchShop.Application.Features.Customers;
using WatchShop.Domain.Entities;

namespace WatchShop.Infrastructure.Services;

public class CustomerQueryService : ICustomerQueryService
{
    private readonly ISqlSugarClient _db;

    public CustomerQueryService(ISqlSugarClient db) => _db = db;

    public async Task<PagedResult<CustomerAdminResponse>> GetPagedAsync(
        int page,
        int pageSize,
        string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Queryable<Customer>().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            query = query.Where(x =>
                x.Username.Contains(k) ||
                (x.Nickname != null && x.Nickname.Contains(k)) ||
                (x.Phone != null && x.Phone.Contains(k)) ||
                (x.Email != null && x.Email.Contains(k)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<CustomerAdminResponse>
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items.Select(x => new CustomerAdminResponse
            {
                Id = x.Id,
                Username = x.Username,
                Nickname = x.Nickname,
                Phone = x.Phone,
                Email = x.Email,
                IsEnabled = x.IsEnabled,
                CreatedAt = x.CreatedAt,
            }).ToList(),
        };
    }
}
