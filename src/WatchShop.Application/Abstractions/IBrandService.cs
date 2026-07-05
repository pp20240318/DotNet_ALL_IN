using WatchShop.Application.Common;
using WatchShop.Application.Features.Brands.Dtos;

namespace WatchShop.Application.Abstractions;

public interface IBrandService
{
    Task<long> CreateAsync(BrandCreateRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(long id, BrandUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<BrandResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<PagedResult<BrandResponse>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
