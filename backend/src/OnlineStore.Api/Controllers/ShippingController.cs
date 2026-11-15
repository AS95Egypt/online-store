using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStore.Application.Shipping;

namespace OnlineStore.Api.Controllers;

[ApiController]
[Route("api/shipping")]
[AllowAnonymous]
public sealed class ShippingController : ControllerBase
{
    private readonly IShippingService _shippingService;

    public ShippingController(IShippingService shippingService)
    {
        _shippingService = shippingService;
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetOptionsAsync(CancellationToken cancellationToken) =>
        Ok(await _shippingService.GetOptionsAsync(cancellationToken));
}
