using Microsoft.AspNetCore.Mvc;
using WatchShop.Application.Abstractions;

namespace WatchShop.Admin.Api.Controllers;


[Route("health")]
public class HealthController : ApiControllerBase
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
        return Success(new
        {
            database = "dotnet_all_in1",
            mysqlVersion = version
        });
    }
}