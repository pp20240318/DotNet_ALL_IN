using MediatR;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Common;

namespace WatchShop.Application.Features.Categories;

public record GetCategoriesPagedQuery(int Page = 1, int PageSize = 10) : IRequest<PagedResult<CategoryResponse>>;
public record GetCategoryByIdQuery(long Id) : IRequest<CategoryResponse?>;
public record CreateCategoryCommand(CategoryCreateRequest Request) : IRequest<long>;
public record UpdateCategoryCommand(long Id, CategoryUpdateRequest Request) : IRequest;
public record DeleteCategoryCommand(long Id) : IRequest;

public class GetCategoriesPagedQueryHandler(ICategoryService service)
    : IRequestHandler<GetCategoriesPagedQuery, PagedResult<CategoryResponse>>
{
    public Task<PagedResult<CategoryResponse>> Handle(GetCategoriesPagedQuery request, CancellationToken cancellationToken)
        => service.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
}

public class GetCategoryByIdQueryHandler(ICategoryService service)
    : IRequestHandler<GetCategoryByIdQuery, CategoryResponse?>
{
    public Task<CategoryResponse?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        => service.GetByIdAsync(request.Id, cancellationToken);
}

public class CreateCategoryCommandHandler(ICategoryService service)
    : IRequestHandler<CreateCategoryCommand, long>
{
    public Task<long> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        => service.CreateAsync(request.Request, cancellationToken);
}

public class UpdateCategoryCommandHandler(ICategoryService service)
    : IRequestHandler<UpdateCategoryCommand>
{
    public Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        => service.UpdateAsync(request.Id, request.Request, cancellationToken);
}

public class DeleteCategoryCommandHandler(ICategoryService service)
    : IRequestHandler<DeleteCategoryCommand>
{
    public Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        => service.DeleteAsync(request.Id, cancellationToken);
}
