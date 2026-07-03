using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace WatchShop.Admin.Api.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        private readonly ISqlSugarClient _db;

        public HealthController(ISqlSugarClient db)
        {
            _db = db;
        }

        [HttpGet("db")]
        public async Task<IActionResult> CheckDatabase()
        {
            var version = await _db.Ado.GetStringAsync("SELECT VERSION()");
            return Ok(new
            {
                status = "ok",
                database = "dotnet_all_in1",
                mysqlVersion = version
            });
        }
    }
}
