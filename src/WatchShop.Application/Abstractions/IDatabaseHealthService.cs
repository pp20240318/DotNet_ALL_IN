using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchShop.Application.Abstractions;

/// <summary>
/// 数据库健康检查服务（Application 层接口，不含 SqlSugar 依赖）
/// </summary>
public interface IDatabaseHealthService
{
    /// <summary>
    /// 获取 MySQL 版本号，用于验证数据库连通性
    /// </summary>
    Task<string> GetMySqlVersionAsync();
}
