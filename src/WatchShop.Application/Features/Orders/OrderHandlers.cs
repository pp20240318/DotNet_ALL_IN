using MediatR;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Common;

namespace WatchShop.Application.Features.Orders;

public record GetOrdersPagedQuery(OrderQueryRequest Query) : IRequest<PagedResult<OrderListResponse>>;
public record GetOrderByIdQuery(long Id) : IRequest<OrderDetailResponse?>;
public record CreateDemoOrderCommand() : IRequest<long>;
public record ShipOrderCommand(long Id) : IRequest;
public record CancelOrderCommand(long Id) : IRequest;

public class GetOrdersPagedQueryHandler(IOrderService service)
    : IRequestHandler<GetOrdersPagedQuery, PagedResult<OrderListResponse>>
{
    public Task<PagedResult<OrderListResponse>> Handle(GetOrdersPagedQuery request, CancellationToken cancellationToken)
        => service.GetPagedAsync(request.Query, cancellationToken);
}

public class GetOrderByIdQueryHandler(IOrderService service)
    : IRequestHandler<GetOrderByIdQuery, OrderDetailResponse?>
{
    public Task<OrderDetailResponse?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        => service.GetByIdAsync(request.Id, cancellationToken);
}

public class CreateDemoOrderCommandHandler(IOrderService service)
    : IRequestHandler<CreateDemoOrderCommand, long>
{
    public Task<long> Handle(CreateDemoOrderCommand request, CancellationToken cancellationToken)
        => service.CreateDemoOrderAsync(cancellationToken);
}

public class ShipOrderCommandHandler(IOrderService service)
    : IRequestHandler<ShipOrderCommand>
{
    public Task Handle(ShipOrderCommand request, CancellationToken cancellationToken)
        => service.ShipAsync(request.Id, cancellationToken);
}

public class CancelOrderCommandHandler(IOrderService service)
    : IRequestHandler<CancelOrderCommand>
{
    public Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        => service.CancelAsync(request.Id, cancellationToken);
}
