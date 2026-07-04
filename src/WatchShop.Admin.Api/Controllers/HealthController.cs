using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using WatchShop.Application.Abstractions;

namespace WatchShop.Admin.Api.Controllers;


[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    private readonly IDatabaseHealthService _healthService;
    public HealthController(IDatabaseHealthService healthService)
    {
        _healthService = healthService;
    }
    [HttpGet("db")]
    public async Task<IActionResult> CheckDatabase()
    {
        var version = await _healthService.GetMySqlVersionAsync();
        return Ok(new
        {
            status = "ok",
            database = "dotnet_all_in1",
            mysqlVersion = version
        });
    }
}
