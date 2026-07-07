using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchShop.Application.Dtos.Auth;
using WatchShop.Application.Features.Auth;

namespace WatchShop.Admin.Api.Controllers;

[Route("auth")]
public class AuthController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
        => Success(await _mediator.Send(new AdminLoginCommand(request)), "登录成功");

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        => Success(await _mediator.Send(new AdminRefreshCommand(request)), "刷新成功");

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var adminIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(adminIdClaim, out var adminId))
        {
            return Fail(401, "无效的登录状态");
        }

        return Success(await _mediator.Send(new GetAdminProfileQuery(adminId)));
    }
}
