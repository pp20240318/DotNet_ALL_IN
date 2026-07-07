namespace WatchShop.Application.Features.Search;

public class SearchResultItem
{
    public string Type { get; set; } = string.Empty;
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
}

public interface ISearchService
{
    Task<List<SearchResultItem>> SearchAsync(string keyword, int limit = 20, CancellationToken cancellationToken = default);
}
