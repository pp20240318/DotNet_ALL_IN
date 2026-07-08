namespace WatchShop.Application.Features.Customers;

using MediatR;

public class UpdateCustomerRequest
{
    public bool IsEnabled { get; set; } = true;
}

public record UpdateCustomerCommand(long CustomerId, UpdateCustomerRequest Request) : IRequest;
