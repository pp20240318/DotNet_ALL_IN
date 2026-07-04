using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchShop.Application.Exceptions;

/// <summary>
/// 业务异常（可预期的错误，如参数不合法、资源不存在）
/// </summary>
public class BusinessException : Exception
{
    public int Code { get; }
    public BusinessException(int code, string message) : base(message)
    {
        Code = code;
    }
    public BusinessException(string message)
        : this(Common.ApiResultCode.BadRequest, message)
    {
    }
}