using WatchShop.Application.Abstractions;
using WatchShop.Application.Common;
using WatchShop.Application.Contracts.Persistence;
using WatchShop.Application.Exceptions;
using WatchShop.Application.Features.Brands.Dtos;
using WatchShop.Domain.Entities;

namespace WatchShop.Infrastructure.Services;

public class BrandService : IBrandService
{
    private readonly IUnitOfWork _unitOfWork;

    public BrandService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<long> CreateAsync(BrandCreateRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Brand
        {
            Name = request.Name,
            LogoUrl = request.LogoUrl,
            Description = request.Description,
            SortOrder = request.SortOrder,
            IsEnabled = request.IsEnabled
        };
        return await _unitOfWork.Repository<Brand>().InsertAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(long id, BrandUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Brand>();
        var entity = await repo.GetByIdAsync(id, cancellationToken)
            ?? throw new BusinessException("品牌不存在");

        entity.Name = request.Name;
        entity.LogoUrl = request.LogoUrl;
        entity.Description = request.Description;
        entity.SortOrder = request.SortOrder;
        entity.IsEnabled = request.IsEnabled;
        await repo.UpdateAsync(entity, cancellationToken);
    }

    public Task DeleteAsync(long id, CancellationToken cancellationToken = default)
        => _unitOfWork.Repository<Brand>().SoftDeleteAsync(id, cancellationToken);

    public async Task<BrandResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<Brand>().GetByIdAsync(id, cancellationToken);
        return entity is null ? null : Map(entity);
    }

    public async Task<PagedResult<BrandResponse>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var paged = await _unitOfWork.Repository<Brand>().GetPagedAsync(page, pageSize, cancellationToken: cancellationToken);
        return new PagedResult<BrandResponse>
        {
            Page = paged.Page,
            PageSize = paged.PageSize,
            Total = paged.Total,
            Items = paged.Items.Select(Map).ToList()
        };
    }

    private static BrandResponse Map(Brand entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        LogoUrl = entity.LogoUrl,
        Description = entity.Description,
        SortOrder = entity.SortOrder,
        IsEnabled = entity.IsEnabled,
        Version = entity.Version
    };
}
