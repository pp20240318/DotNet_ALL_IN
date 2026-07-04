using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchShop.Application.Common;

/// <summary>
/// 统一 API 响应格式
/// </summary>
public class ApiResult<T>
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public static ApiResult<T> Ok(T data, string message = "success")
    {
        return new ApiResult<T>
        {
            Code = ApiResultCode.Success,
            Message = message,
            Data = data
        };
    }
    public static ApiResult<T> Fail(int code, string message)
    {
        return new ApiResult<T>
        {
            Code = code,
            Message = message,
            Data = default
        };
    }
}