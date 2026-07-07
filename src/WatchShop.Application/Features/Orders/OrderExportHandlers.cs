using MediatR;
using WatchShop.Application.Abstractions;
using WatchShop.Domain.Enums;

namespace WatchShop.Application.Features.Orders;

public record ExportOrdersQuery(OrderStatus? Status = null, int MaxRows = 5000) : IRequest<byte[]>;

public class ExportOrdersQueryHandler(IOrderService service)
    : IRequestHandler<ExportOrdersQuery, byte[]>
{
    public Task<byte[]> Handle(ExportOrdersQuery request, CancellationToken cancellationToken)
        => service.ExportCsvAsync(request.Status, request.MaxRows, cancellationToken);
}
