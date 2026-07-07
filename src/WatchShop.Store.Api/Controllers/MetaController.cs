using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WatchShop.Store.Api.Controllers;

[Route("api")]
[ApiVersion("1.0")]
public class MetaController : ControllerBase
{
    [HttpGet("version")]
    [AllowAnonymous]
    public IActionResult GetVersion()
        => Ok(new
        {
            name = "WatchShop.Store.Api",
            version = "1.0",
            framework = "net8.0"
        });
}
