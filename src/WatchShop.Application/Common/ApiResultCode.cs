using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchShop.Application.Common;

/// <summary>
/// API 业务状态码（与 HTTP 状态码分开；0 表示成功）
/// </summary>
public static class ApiResultCode
{
    public const int Success = 0;
    public const int BadRequest = 400;
    public const int Unauthorized = 401;
    public const int Forbidden = 403;
    public const int NotFound = 404;
    public const int ValidationError = 422;
    public const int InternalError = 500;
}