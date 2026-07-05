using WatchShop.Application.Abstractions;
using WatchShop.Application.Common;
using WatchShop.Application.Exceptions;
using WatchShop.Domain.Entities;

namespace WatchShop.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<long> CreateAsync(CategoryCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Category
        {
            Name = request.Name,
            ParentId = request.ParentId,
            SortOrder = request.SortOrder,
            IsEnabled = request.IsEnabled
        };
        return await _unitOfWork.Repository<Category>().InsertAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(long id, CategoryUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Category>();
        var entity = await repo.GetByIdAsync(id, cancellationToken)
            ?? throw new BusinessException("分类不存在");

        entity.Name = request.Name;
        entity.ParentId = request.ParentId;
        entity.SortOrder = request.SortOrder;
        entity.IsEnabled = request.IsEnabled;
        await repo.UpdateAsync(entity, cancellationToken);
    }

    public Task DeleteAsync(long id, CancellationToken cancellationToken = default)
        => _unitOfWork.Repository<Category>().SoftDeleteAsync(id, cancellationToken);

    public async Task<CategoryResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<Category>().GetByIdAsync(id, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    public async Task<PagedResult<CategoryResponse>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var paged = await _unitOfWork.Repository<Category>().GetPagedAsync(page, pageSize, cancellationToken: cancellationToken);
        return new PagedResult<CategoryResponse>
        {
            Page = paged.Page,
            PageSize = paged.PageSize,
            Total = paged.Total,
            Items = paged.Items.Select(Map).ToList()
        };
    }

    private static CategoryResponse Map(Category entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        ParentId = entity.ParentId,
        SortOrder = entity.SortOrder,
        IsEnabled = entity.IsEnabled,
        Version = entity.Version
    };
}
