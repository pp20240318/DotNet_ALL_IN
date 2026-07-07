using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
using WatchShop.Application.Features.Search;
using WatchShop.Application.Options;
using WatchShop.Domain.Entities;
using WatchShop.Domain.Enums;

namespace WatchShop.Infrastructure.Search;

public class ElasticsearchSearchService : ISearchService
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticsearchOptions _options;
    private readonly ISqlSugarClient _db;
    private readonly ILogger<ElasticsearchSearchService> _logger;

    public ElasticsearchSearchService(
        IOptions<ElasticsearchOptions> options,
        ISqlSugarClient db,
        ILogger<ElasticsearchSearchService> logger)
    {
        _options = options.Value;
        _db = db;
        _logger = logger;
        var settings = new ElasticsearchClientSettings(new Uri(_options.Uri));
        _client = new ElasticsearchClient(settings);
    }

    public async Task<List<SearchResultItem>> SearchAsync(string keyword, int limit = 20, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return [];
        }

        try
        {
            var response = await _client.SearchAsync<SearchDocument>(s => s
                .Indices(_options.IndexName)
                .Size(limit)
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Query(keyword)
                        .Fields(new[] { "title", "subtitle" }))));

            if (!response.IsValidResponse || response.Documents is null)
            {
                _logger.LogWarning("Elasticsearch search failed, fallback to empty result.");
                return [];
            }

            return response.Documents.Select(x => new SearchResultItem
            {
                Type = x.Type,
                Id = x.EntityId,
                Title = x.Title,
                Subtitle = x.Subtitle
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Elasticsearch search exception.");
            return [];
        }
    }

    public async Task SyncIndexAsync(CancellationToken cancellationToken = default)
    {
        var exists = await _client.Indices.ExistsAsync(_options.IndexName, cancellationToken);
        if (!exists.Exists)
        {
            await _client.Indices.CreateAsync(_options.IndexName, cancellationToken);
        }

        var docs = new List<SearchDocument>();
        var products = await _db.Queryable<Product>().Where(x => !x.IsDeleted).ToListAsync();
        docs.AddRange(products.Select(x => new SearchDocument
        {
            Type = "product",
            EntityId = x.Id,
            Title = x.Name,
            Subtitle = x.Status.ToString()
        }));

        var brands = await _db.Queryable<Brand>().Where(x => !x.IsDeleted).ToListAsync();
        docs.AddRange(brands.Select(x => new SearchDocument
        {
            Type = "brand",
            EntityId = x.Id,
            Title = x.Name
        }));

        var orders = await _db.Queryable<ShopOrder>().Where(x => !x.IsDeleted).Take(500).ToListAsync();
        docs.AddRange(orders.Select(x => new SearchDocument
        {
            Type = "order",
            EntityId = x.Id,
            Title = x.OrderNo,
            Subtitle = x.Status.ToString()
        }));

        foreach (var doc in docs)
        {
            await _client.IndexAsync(doc, idx => idx
                .Index(_options.IndexName)
                .Id($"{doc.Type}:{doc.EntityId}"), cancellationToken);
        }
    }

    public class SearchDocument
    {
        public string Type { get; set; } = string.Empty;
        public long EntityId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
    }
}

public class CompositeSearchService : ISearchService
{
    private readonly ElasticsearchOptions _options;
    private readonly ElasticsearchSearchService _elasticSearch;
    private readonly Services.SearchService _fallback;

    public CompositeSearchService(
        IOptions<ElasticsearchOptions> options,
        ElasticsearchSearchService elasticSearch,
        Services.SearchService fallback)
    {
        _options = options.Value;
        _elasticSearch = elasticSearch;
        _fallback = fallback;
    }

    public async Task<List<SearchResultItem>> SearchAsync(string keyword, int limit = 20, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return await _fallback.SearchAsync(keyword, limit, cancellationToken);
        }

        var results = await _elasticSearch.SearchAsync(keyword, limit, cancellationToken);
        return results.Count > 0
            ? results
            : await _fallback.SearchAsync(keyword, limit, cancellationToken);
    }
}
