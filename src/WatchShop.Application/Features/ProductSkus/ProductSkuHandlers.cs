using MediatR;
using WatchShop.Application.Abstractions;

namespace WatchShop.Application.Features.ProductSkus;

public record GetSkusByProductQuery(long ProductId) : IRequest<List<ProductSkuResponse>>;
public record GetSkuByIdQuery(long Id) : IRequest<ProductSkuResponse?>;
public record CreateSkuCommand(ProductSkuCreateRequest Request) : IRequest<long>;
public record UpdateSkuCommand(long Id, ProductSkuUpdateRequest Request) : IRequest;
public record StockInCommand(long Id, int Quantity) : IRequest;
public record StockOutCommand(long Id, int Quantity) : IRequest;
public record DeleteSkuCommand(long Id) : IRequest;

public class GetSkusByProductQueryHandler(IProductSkuService service)
    : IRequestHandler<GetSkusByProductQuery, List<ProductSkuResponse>>
{
    public Task<List<ProductSkuResponse>> Handle(GetSkusByProductQuery request, CancellationToken cancellationToken)
        => service.GetByProductIdAsync(request.ProductId, cancellationToken);
}

public class GetSkuByIdQueryHandler(IProductSkuService service)
    : IRequestHandler<GetSkuByIdQuery, ProductSkuResponse?>
{
    public Task<ProductSkuResponse?> Handle(GetSkuByIdQuery request, CancellationToken cancellationToken)
        => service.GetByIdAsync(request.Id, cancellationToken);
}

public class CreateSkuCommandHandler(IProductSkuService service)
    : IRequestHandler<CreateSkuCommand, long>
{
    public Task<long> Handle(CreateSkuCommand request, CancellationToken cancellationToken)
        => service.CreateAsync(request.Request, cancellationToken);
}

public class UpdateSkuCommandHandler(IProductSkuService service)
    : IRequestHandler<UpdateSkuCommand>
{
    public Task Handle(UpdateSkuCommand request, CancellationToken cancellationToken)
        => service.UpdateAsync(request.Id, request.Request, cancellationToken);
}

public class StockInCommandHandler(IProductSkuService service)
    : IRequestHandler<StockInCommand>
{
    public Task Handle(StockInCommand request, CancellationToken cancellationToken)
        => service.StockInAsync(request.Id, request.Quantity, cancellationToken);
}

public class StockOutCommandHandler(IProductSkuService service)
    : IRequestHandler<StockOutCommand>
{
    public Task Handle(StockOutCommand request, CancellationToken cancellationToken)
        => service.StockOutAsync(request.Id, request.Quantity, cancellationToken);
}

public class DeleteSkuCommandHandler(IProductSkuService service)
    : IRequestHandler<DeleteSkuCommand>
{
    public Task Handle(DeleteSkuCommand request, CancellationToken cancellationToken)
        => service.DeleteAsync(request.Id, cancellationToken);
}
