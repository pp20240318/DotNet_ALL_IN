using MediatR;
using WatchShop.Application.Features.Search;

namespace WatchShop.Application.Features.Search;

public record GlobalSearchQuery(string Keyword, int Limit = 20) : IRequest<List<SearchResultItem>>;

public class GlobalSearchQueryHandler(ISearchService service)
    : IRequestHandler<GlobalSearchQuery, List<SearchResultItem>>
{
    public Task<List<SearchResultItem>> Handle(GlobalSearchQuery request, CancellationToken cancellationToken)
        => service.SearchAsync(request.Keyword, request.Limit, cancellationToken);
}
