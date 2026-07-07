using MediatR;
using WatchShop.Application.Abstractions;

namespace WatchShop.Application.Features.Rbac;

public record CreateAdminCommand(CreateAdminRequest Request) : IRequest<long>;

public record UpdateAdminCommand(long AdminId, UpdateAdminRequest Request) : IRequest;

public class CreateAdminCommandHandler(IAdminManagementService service)
    : IRequestHandler<CreateAdminCommand, long>
{
    public Task<long> Handle(CreateAdminCommand request, CancellationToken cancellationToken)
        => service.CreateAsync(request.Request, cancellationToken);
}

public class UpdateAdminCommandHandler(IAdminManagementService service)
    : IRequestHandler<UpdateAdminCommand>
{
    public Task Handle(UpdateAdminCommand request, CancellationToken cancellationToken)
        => service.UpdateAsync(request.AdminId, request.Request, cancellationToken);
}
