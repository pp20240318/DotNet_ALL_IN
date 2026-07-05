using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WatchShop.Application.Abstractions;
using WatchShop.Application.Options;

namespace WatchShop.Admin.Api.Controllers;

[Route("health")]
public class HealthController : ApiControllerBase
{
    private readonly IDatabaseHealthService _healthService;
    private readonly DatabaseOptions _dbOptions;

    public HealthController(
        IDatabaseHealthService healthService,
        IOptions<DatabaseOptions> dbOptions)
    {
        _healthService = healthService;
        _dbOptions = dbOptions.Value;
    }

    [HttpGet("db")]
    public async Task<IActionResult> CheckDatabase()
    {
        var version = await _healthService.GetMySqlVersionAsync();
        return Success(new
        {
            database = _dbOptions.Database,
            mysqlVersion = version
        });
    }
}
