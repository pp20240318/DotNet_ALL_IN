using MediatR;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Common;
using WatchShop.Domain.Enums;

namespace WatchShop.Application.Features.Products;

public record GetProductsPagedQuery(ProductQueryRequest Query) : IRequest<PagedResult<ProductResponse>>;
public record GetProductByIdQuery(long Id) : IRequest<ProductResponse?>;
public record CreateProductCommand(ProductCreateRequest Request) : IRequest<long>;
public record UpdateProductCommand(long Id, ProductUpdateRequest Request) : IRequest;
public record ChangeProductStatusCommand(long Id, ProductStatus Status) : IRequest;
public record DeleteProductCommand(long Id) : IRequest;

public class GetProductsPagedQueryHandler(IProductService service)
    : IRequestHandler<GetProductsPagedQuery, PagedResult<ProductResponse>>
{
    public Task<PagedResult<ProductResponse>> Handle(GetProductsPagedQuery request, CancellationToken cancellationToken)
        => service.GetPagedAsync(request.Query, cancellationToken);
}

public class GetProductByIdQueryHandler(IProductService service)
    : IRequestHandler<GetProductByIdQuery, ProductResponse?>
{
    public Task<ProductResponse?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        => service.GetByIdAsync(request.Id, cancellationToken);
}

public class CreateProductCommandHandler(IProductService service)
    : IRequestHandler<CreateProductCommand, long>
{
    public Task<long> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        => service.CreateAsync(request.Request, cancellationToken);
}

public class UpdateProductCommandHandler(IProductService service)
    : IRequestHandler<UpdateProductCommand>
{
    public Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        => service.UpdateAsync(request.Id, request.Request, cancellationToken);
}

public class ChangeProductStatusCommandHandler(IProductService service)
    : IRequestHandler<ChangeProductStatusCommand>
{
    public Task Handle(ChangeProductStatusCommand request, CancellationToken cancellationToken)
        => service.ChangeStatusAsync(request.Id, request.Status, cancellationToken);
}

public class DeleteProductCommandHandler(IProductService service)
    : IRequestHandler<DeleteProductCommand>
{
    public Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        => service.DeleteAsync(request.Id, cancellationToken);
}
