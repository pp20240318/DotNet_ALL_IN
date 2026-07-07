using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchShop.Admin.Api.Authorization;
using WatchShop.Application.Authorization;
using WatchShop.Application.Features.Customers;

namespace WatchShop.Admin.Api.Controllers;

[Route("customers")]
[Authorize]
public class CustomerController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CustomerController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequirePermission(AppPermissions.CustomerRead)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? keyword = null)
        => Success(await _mediator.Send(new GetCustomersPagedQuery(page, pageSize, keyword)));
}
