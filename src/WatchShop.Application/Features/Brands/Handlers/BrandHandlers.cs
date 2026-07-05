using MediatR;
using WatchShop.Application.Common;
using WatchShop.Application.Contracts.Persistence;
using WatchShop.Application.Exceptions;
using WatchShop.Application.Features.Brands.Commands;
using WatchShop.Application.Features.Brands.Dtos;
using WatchShop.Application.Features.Brands.Queries;
using WatchShop.Domain.Entities;

namespace WatchShop.Application.Features.Brands.Handlers;

public class GetBrandsPagedQueryHandler : IRequestHandler<GetBrandsPagedQuery, PagedResult<BrandResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBrandsPagedQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<BrandResponse>> Handle(GetBrandsPagedQuery request, CancellationToken cancellationToken)
    {
        var paged = await _unitOfWork.Repository<Brand>()
            .GetPagedAsync(request.Page, request.PageSize, cancellationToken: cancellationToken);

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

public class GetBrandByIdQueryHandler : IRequestHandler<GetBrandByIdQuery, BrandResponse?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBrandByIdQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<BrandResponse?> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.Repository<Brand>().GetByIdAsync(request.Id, cancellationToken);
        return entity is null ? null : new BrandResponse
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
}

public class CreateBrandCommandHandler : IRequestHandler<CreateBrandCommand, long>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateBrandCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<long> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var entity = new Brand
        {
            Name = request.Request.Name,
            LogoUrl = request.Request.LogoUrl,
            Description = request.Request.Description,
            SortOrder = request.Request.SortOrder,
            IsEnabled = request.Request.IsEnabled
        };
        return await _unitOfWork.Repository<Brand>().InsertAsync(entity, cancellationToken);
    }
}

public class UpdateBrandCommandHandler : IRequestHandler<UpdateBrandCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBrandCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Brand>();
        var entity = await repo.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new BusinessException("品牌不存在");

        entity.Name = request.Request.Name;
        entity.LogoUrl = request.Request.LogoUrl;
        entity.Description = request.Request.Description;
        entity.SortOrder = request.Request.SortOrder;
        entity.IsEnabled = request.Request.IsEnabled;
        await repo.UpdateAsync(entity, cancellationToken);
    }
}

public class DeleteBrandCommandHandler : IRequestHandler<DeleteBrandCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBrandCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public Task Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
        => _unitOfWork.Repository<Brand>().SoftDeleteAsync(request.Id, cancellationToken);
}
