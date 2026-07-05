using SqlSugar;
using WatchShop.Application.Features.Dashboard;
using WatchShop.Application.Features.Store.Dtos;
using WatchShop.Domain.Entities;
using WatchShop.Domain.Enums;

namespace WatchShop.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly ISqlSugarClient _db;

    public DashboardService(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<DashboardStatsResponse> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var productCount = await _db.Queryable<Product>().Where(x => !x.IsDeleted).CountAsync();
        var orderCount = await _db.Queryable<ShopOrder>().Where(x => !x.IsDeleted).CountAsync();
        var customerCount = await _db.Queryable<Customer>().Where(x => !x.IsDeleted).CountAsync();
        var pendingPaymentCount = await _db.Queryable<ShopOrder>()
            .Where(x => !x.IsDeleted && x.Status == OrderStatus.PendingPayment)
            .CountAsync();
        var todayAmount = await _db.Queryable<ShopOrder>()
            .Where(x => !x.IsDeleted && x.CreatedAt >= today && x.CreatedAt < tomorrow)
            .SumAsync(x => x.TotalAmount);

        return new DashboardStatsResponse
        {
            ProductCount = productCount,
            OrderCount = orderCount,
            CustomerCount = customerCount,
            PendingPaymentCount = pendingPaymentCount,
            TodayOrderAmount = todayAmount
        };
    }
}
