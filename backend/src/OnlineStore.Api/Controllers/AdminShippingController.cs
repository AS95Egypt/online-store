using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStore.Application.Shipping;

namespace OnlineStore.Api.Controllers;

/// <summary>Administrator endpoints for governorate shipping fees.</summary>
[ApiController]
[Route("api/admin/shipping")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminShippingController : ControllerBase
{
    private readonly IAdminShippingService _shippingService;

    public AdminShippingController(IAdminShippingService shippingService)
    {
        _shippingService = shippingService;
    }

    [HttpGet("governorates")]
    public async Task<IActionResult> GetGovernoratesAsync(CancellationToken cancellationToken)
    {
        var governorates = await _shippingService.GetGovernoratesAsync(cancellationToken);
        return Ok(new { success = true, data = governorates });
    }

    [HttpPut("governorates/{id:int}")]
    public async Task<IActionResult> UpdateFeeAsync(
        int id,
        [FromBody] UpdateGovernorateFeeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var governorate = await _shippingService.UpdateFeeAsync(id, request.ShippingFee, cancellationToken);
            return governorate is null
                ? NotFound(new { success = false, error = new { code = "GOVERNORATE_NOT_FOUND", message = "Governorate not found." } })
                : Ok(new { success = true, data = governorate });
        }
        catch (ShippingFeeValidationException exception)
        {
            return BadRequest(new
            {
                success = false,
                error = new
                {
                    code = exception.Code,
                    message = exception.Message,
                    details = exception.Errors,
                },
            });
        }
    }
}
