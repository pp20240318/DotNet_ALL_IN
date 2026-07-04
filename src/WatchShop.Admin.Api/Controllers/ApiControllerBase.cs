using Microsoft.AspNetCore.Mvc;
using WatchShop.Application.Common;

namespace WatchShop.Admin.Api.Controllers;

/// <summary>
/// Controller 基类，提供统一成功/失败响应
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Success<T>(T data, string message = "success")
    {
        return Ok(ApiResult<T>.Ok(data, message));
    }
    protected IActionResult Fail(int code, string message)
    {
        return BadRequest(ApiResult<object>.Fail(code, message));
    }
}