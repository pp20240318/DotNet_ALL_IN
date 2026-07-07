using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchShop.Admin.Api.Authorization;
using WatchShop.Application.Authorization;
using WatchShop.Application.Features.Rbac;

namespace WatchShop.Admin.Api.Controllers;

[Route("roles")]
[Authorize]
public class RoleController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public RoleController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequirePermission(AppPermissions.SystemAdmin)]
    public async Task<IActionResult> GetAll()
        => Success(await _mediator.Send(new GetAllRolesQuery()));

    [HttpGet("admins")]
    [RequirePermission(AppPermissions.SystemAdmin)]
    public async Task<IActionResult> GetAdmins()
        => Success(await _mediator.Send(new GetAllAdminsQuery()));

    [HttpPut("admins/{adminId:long}/roles")]
    [RequirePermission(AppPermissions.SystemAdmin)]
    public async Task<IActionResult> SetAdminRoles(long adminId, [FromBody] SetAdminRolesRequest request)
    {
        await _mediator.Send(new SetAdminRolesCommand(adminId, request.Roles));
        return Success(true, "角色分配成功");
    }
}
