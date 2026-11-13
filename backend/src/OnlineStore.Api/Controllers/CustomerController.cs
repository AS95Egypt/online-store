using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStore.Application.Customers;

namespace OnlineStore.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet("me")]
    public async Task<IActionResult> GetProfileAsync()
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _customerService.GetProfileAsync(userId);
        if (result.NotFound)
        {
            return NotFound();
        }

        if (result.Value is null)
        {
            return Problem("The customer profile could not be loaded.");
        }

        return Ok(result.Value);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfileAsync([FromBody] UpdateCustomerProfileRequest request)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _customerService.UpdateProfileAsync(userId, request);
        if (result.NotFound)
        {
            return NotFound();
        }

        if (result.ValidationErrors is not null)
        {
            return ValidationProblem(new ValidationProblemDetails(
                result.ValidationErrors.ToDictionary(error => error.Key, error => error.Value)));
        }

        if (result.ErrorMessage is not null)
        {
            return BadRequest(new { success = false, message = result.ErrorMessage });
        }

        if (result.Value is null)
        {
            return Problem("The customer profile could not be updated.");
        }

        return Ok(result.Value);
    }

    [HttpPost("me/change-password")]
    public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordRequest request)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _customerService.ChangePasswordAsync(userId, request);
        if (result.NotFound)
        {
            return NotFound();
        }

        if (result.ValidationErrors is not null)
        {
            return ValidationProblem(new ValidationProblemDetails(
                result.ValidationErrors.ToDictionary(error => error.Key, error => error.Value)));
        }

        if (result.ErrorMessage is not null)
        {
            return BadRequest(new { success = false, message = result.ErrorMessage });
        }

        if (!result.Succeeded)
        {
            return Problem("The password could not be changed.");
        }

        return Ok(new { success = true, message = "Password changed successfully." });
    }
}
