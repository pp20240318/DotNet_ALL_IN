using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using WatchShop.Domain.Entities;
using WatchShop.Infrastructure.Services;

namespace WatchShop.Admin.Api.Filters;

public class OperationLogFilter : IAsyncActionFilter
{
    private readonly OperationLogService _operationLogService;

    public OperationLogFilter(OperationLogService operationLogService)
    {
        _operationLogService = operationLogService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();
        if (executed.Exception is not null)
        {
            return;
        }

        var httpContext = context.HttpContext;
        if (httpContext.Request.Method is not ("POST" or "PUT" or "DELETE"))
        {
            return;
        }

        var adminIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        long? adminId = long.TryParse(adminIdClaim, out var parsedId) ? parsedId : null;
        var adminName = httpContext.User.Identity?.Name ?? "anonymous";
        var controller = context.RouteData.Values["controller"]?.ToString() ?? "unknown";
        var action = context.RouteData.Values["action"]?.ToString() ?? "unknown";

        await _operationLogService.WriteAsync(new OperationLog
        {
            AdminId = adminId,
            AdminName = adminName,
            Module = controller,
            Action = action,
            RequestPath = httpContext.Request.Path,
            RequestMethod = httpContext.Request.Method,
            IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
            IsSuccess = executed.Result is ObjectResult { StatusCode: null or >= 200 and < 300 },
            Message = "操作成功"
        });
    }
}

public class OperationLogFilterAttribute : TypeFilterAttribute
{
    public OperationLogFilterAttribute() : base(typeof(OperationLogFilter))
    {
    }
}
