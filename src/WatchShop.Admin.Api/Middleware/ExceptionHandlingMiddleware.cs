using System.Net;
using WatchShop.Application.Common;
using WatchShop.Application.Exceptions;
namespace WatchShop.Admin.Api.Middleware;


/// <summary>
/// 全局异常处理中间件
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BusinessException ex)
        {
            // 可预期的业务错误 → 400
            await WriteErrorAsync(context, HttpStatusCode.BadRequest, ex.Code, ex.Message);
        }
        catch (Exception ex)
        {
            // 未预期错误 → 500，记录日志，不暴露细节
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await WriteErrorAsync(
                context,
                HttpStatusCode.InternalServerError,
                ApiResultCode.InternalError,
                "服务器内部错误");
        }
    }
    private static async Task WriteErrorAsync(
        HttpContext context,
        HttpStatusCode httpStatus,
        int code,
        string message)
    {
        context.Response.StatusCode = (int)httpStatus;
        context.Response.ContentType = "application/json";
        var result = ApiResult<object>.Fail(code, message);
        await context.Response.WriteAsJsonAsync(result);
    }
}
/// <summary>
/// 中间件注册扩展
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}