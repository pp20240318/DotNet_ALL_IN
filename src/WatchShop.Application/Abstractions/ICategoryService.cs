using WatchShop.Application.Common;

namespace WatchShop.Application.Abstractions;

public interface ICategoryService
{
    Task<long> CreateAsync(CategoryCreateRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(long id, CategoryUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<CategoryResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<PagedResult<CategoryResponse>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

public class CategoryCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    public int SortOrder { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public class CategoryUpdateRequest : CategoryCreateRequest { }

public class CategoryResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    public int SortOrder { get; set; }
    public bool IsEnabled { get; set; }
    public int Version { get; set; }
}
