using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatchShop.Application.Abstractions;

namespace WatchShop.Infrastructure.Services;

/// <summary>
/// 数据库健康检查实现（SqlSugar 细节封装在此层）
/// </summary>
public class DatabaseHealthService : IDatabaseHealthService
{
    private readonly ISqlSugarClient _db;
    // 构造函数注入：DI 容器自动传入 ISqlSugarClient
    public DatabaseHealthService(ISqlSugarClient db)
    {
        _db = db;
    }
    public async Task<string> GetMySqlVersionAsync()
    {
        return await _db.Ado.GetStringAsync("SELECT VERSION()");
    }
}
