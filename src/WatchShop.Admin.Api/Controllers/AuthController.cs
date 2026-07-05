using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Dtos.Auth;

namespace WatchShop.Admin.Api.Controllers;

[Route("auth")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// 管理员登录（无需 Token）
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Success(result, "登录成功");
    }

    /// <summary>
    /// 获取当前登录管理员信息（需要 Token）
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var adminIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(adminIdClaim, out var adminId))
        {
            return Fail(401, "无效的登录状态");
        }

        var profile = await _authService.GetProfileAsync(adminId);
        return Success(profile);
    }
}
