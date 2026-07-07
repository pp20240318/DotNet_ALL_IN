using SqlSugar;
using WatchShop.Application.Features.Search;
using WatchShop.Domain.Entities;
using WatchShop.Domain.Enums;

namespace WatchShop.Infrastructure.Services;

public class SearchService : ISearchService
{
    private readonly ISqlSugarClient _db;

    public SearchService(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<List<SearchResultItem>> SearchAsync(string keyword, int limit = 20, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return [];
        }

        var results = new List<SearchResultItem>();
        var pattern = $"%{keyword.Trim()}%";

        var products = await _db.Queryable<Product>()
            .Where(x => !x.IsDeleted && x.Name.Contains(keyword))
            .Take(limit)
            .ToListAsync();

        results.AddRange(products.Select(x => new SearchResultItem
        {
            Type = "product",
            Id = x.Id,
            Title = x.Name,
            Subtitle = x.Status.ToString()
        }));

        var brands = await _db.Queryable<Brand>()
            .Where(x => !x.IsDeleted && x.Name.Contains(keyword))
            .Take(limit)
            .ToListAsync();

        results.AddRange(brands.Select(x => new SearchResultItem
        {
            Type = "brand",
            Id = x.Id,
            Title = x.Name
        }));

        var orders = await _db.Queryable<ShopOrder>()
            .Where(x => !x.IsDeleted && x.OrderNo.Contains(keyword))
            .Take(limit)
            .ToListAsync();

        results.AddRange(orders.Select(x => new SearchResultItem
        {
            Type = "order",
            Id = x.Id,
            Title = x.OrderNo,
            Subtitle = x.Status.ToString()
        }));

        return results.Take(limit).ToList();
    }
}
