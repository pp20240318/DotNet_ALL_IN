using MediatR;
using WatchShop.Application.Common;

namespace WatchShop.Application.Features.Customers;

public record GetCustomersPagedQuery(int Page = 1, int PageSize = 20, string? Keyword = null)
    : IRequest<PagedResult<CustomerAdminResponse>>;

public class GetCustomersPagedQueryHandler(ICustomerQueryService service)
    : IRequestHandler<GetCustomersPagedQuery, PagedResult<CustomerAdminResponse>>
{
    public Task<PagedResult<CustomerAdminResponse>> Handle(
        GetCustomersPagedQuery request,
        CancellationToken cancellationToken)
        => service.GetPagedAsync(request.Page, request.PageSize, request.Keyword, cancellationToken);
}
