using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using WatchShop.Application.Common;

namespace WatchShop.Store.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Success<T>(T data, string message = "success")
        => Ok(ApiResult<T>.Ok(data, message));

    protected IActionResult Fail(int code, string message)
        => BadRequest(ApiResult<object>.Fail(code, message));
}
