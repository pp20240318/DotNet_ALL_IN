using MediatR;
using WatchShop.Application.Abstractions;
using WatchShop.Domain.Enums;

namespace WatchShop.Application.Features.Products;

public record ExportProductsQuery(ProductStatus? Status = null, int MaxRows = 5000) : IRequest<byte[]>;

public class ExportProductsQueryHandler(IProductService service)
    : IRequestHandler<ExportProductsQuery, byte[]>
{
    public Task<byte[]> Handle(ExportProductsQuery request, CancellationToken cancellationToken)
        => service.ExportCsvAsync(request.Status, request.MaxRows, cancellationToken);
}
