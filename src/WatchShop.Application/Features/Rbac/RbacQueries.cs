using MediatR;
using WatchShop.Application.Abstractions;

namespace WatchShop.Application.Features.Rbac;

public record GetAllRolesQuery() : IRequest<IReadOnlyList<RoleResponse>>;

public record GetAllAdminsQuery() : IRequest<IReadOnlyList<AdminRoleResponse>>;

public class RoleResponse
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public IReadOnlyList<string> Permissions { get; set; } = [];
}

public class AdminRoleResponse
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
}

public class GetAllRolesQueryHandler(IRbacService rbacService)
    : IRequestHandler<GetAllRolesQuery, IReadOnlyList<RoleResponse>>
{
    public Task<IReadOnlyList<RoleResponse>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        => rbacService.GetAllRolesAsync(cancellationToken);
}

public class GetAllAdminsQueryHandler(IRbacService rbacService)
    : IRequestHandler<GetAllAdminsQuery, IReadOnlyList<AdminRoleResponse>>
{
    public Task<IReadOnlyList<AdminRoleResponse>> Handle(GetAllAdminsQuery request, CancellationToken cancellationToken)
        => rbacService.GetAllAdminsWithRolesAsync(cancellationToken);
}
