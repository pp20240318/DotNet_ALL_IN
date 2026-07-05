using MediatR;
using WatchShop.Application.Common;
using WatchShop.Application.Features.Brands.Dtos;

namespace WatchShop.Application.Features.Brands.Queries;

public record GetBrandsPagedQuery(int Page = 1, int PageSize = 10)
    : IRequest<PagedResult<BrandResponse>>;

public record GetBrandByIdQuery(long Id) : IRequest<BrandResponse?>;
