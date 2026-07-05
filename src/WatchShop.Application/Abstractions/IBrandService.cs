using WatchShop.Application.Common;

namespace WatchShop.Application.Abstractions;

public interface IBrandService
{
    Task<long> CreateAsync(BrandCreateRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(long id, BrandUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<BrandResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<PagedResult<BrandResponse>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

public class BrandCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public class BrandUpdateRequest : BrandCreateRequest { }

public class BrandResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsEnabled { get; set; }
    public int Version { get; set; }
}
