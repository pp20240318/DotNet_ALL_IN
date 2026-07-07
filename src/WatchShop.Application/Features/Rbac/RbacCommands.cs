using MediatR;
using WatchShop.Application.Abstractions;

namespace WatchShop.Application.Features.Rbac;

public class SetAdminRolesRequest
{
    public List<string> Roles { get; set; } = [];
}

public record SetAdminRolesCommand(long AdminId, IReadOnlyList<string> Roles) : IRequest;

public class SetAdminRolesCommandHandler(IRbacService rbacService)
    : IRequestHandler<SetAdminRolesCommand>
{
    public Task Handle(SetAdminRolesCommand request, CancellationToken cancellationToken)
        => rbacService.SetAdminRolesAsync(request.AdminId, request.Roles, cancellationToken);
}
