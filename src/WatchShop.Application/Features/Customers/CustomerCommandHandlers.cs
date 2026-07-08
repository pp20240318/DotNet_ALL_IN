using MediatR;

namespace WatchShop.Application.Features.Customers;

public class UpdateCustomerCommandHandler(ICustomerQueryService service)
    : IRequestHandler<UpdateCustomerCommand>
{
    public Task Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        => service.UpdateAsync(request.CustomerId, request.Request, cancellationToken);
}
