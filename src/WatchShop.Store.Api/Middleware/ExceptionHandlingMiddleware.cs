using System.Net;
using WatchShop.Application.Common;
using WatchShop.Application.Exceptions;

namespace WatchShop.Store.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            var httpStatus = ex.Code switch
            {
                ApiResultCode.Unauthorized => HttpStatusCode.Unauthorized,
                ApiResultCode.Forbidden => HttpStatusCode.Forbidden,
                ApiResultCode.NotFound => HttpStatusCode.NotFound,
                _ => HttpStatusCode.BadRequest
            };
            await WriteErrorAsync(context, httpStatus, ex.Code, ex.Message);
        }
        catch (Exception ex)
        {
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
        await context.Response.WriteAsJsonAsync(ApiResult<object>.Fail(code, message));
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}
