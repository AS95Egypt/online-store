using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStore.Application.Orders;
using OnlineStore.Application.Products;

namespace OnlineStore.Api.Controllers;

/// <summary>Administrator endpoints for order operations.</summary>
[ApiController]
[Route("api/admin/orders")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminOrdersController : ControllerBase
{
    private readonly IAdminOrderService _orderService;

    public AdminOrdersController(IAdminOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrdersAsync(
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] string? page,
        [FromQuery] string? pageSize,
        CancellationToken cancellationToken)
    {
        var result = await _orderService.GetOrdersAsync(
            status,
            search,
            ParsePositiveInteger(page, ProductPaginationDefaults.Page),
            ParsePositiveInteger(pageSize, ProductPaginationDefaults.PageSize),
            cancellationToken);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrderDetailsAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderDetailsAsync(id, cancellationToken);
        return order is null
            ? NotFound(new { success = false, error = new { code = "ORDER_NOT_FOUND", message = "Order not found." } })
            : Ok(new { success = true, data = order });
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatusAsync(
        int id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(adminUserId))
        {
            return Unauthorized();
        }

        try
        {
            var order = await _orderService.UpdateStatusAsync(
                id,
                request.Status,
                adminUserId,
                cancellationToken);
            return Ok(new { success = true, data = order });
        }
        catch (AdminOrderConflictException exception)
        {
            return Conflict(Error(exception.Code, exception.Message));
        }
        catch (AdminOrderNotFoundException)
        {
            return OrderNotFound();
        }
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> CancelOrderAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(adminUserId))
        {
            return Unauthorized();
        }

        try
        {
            var order = await _orderService.CancelOrderAsync(id, adminUserId, cancellationToken);
            return Ok(new { success = true, data = order });
        }
        catch (AdminOrderConflictException exception)
        {
            return Conflict(Error(exception.Code, exception.Message));
        }
        catch (AdminOrderNotFoundException)
        {
            return OrderNotFound();
        }
    }

    [HttpPost("{id:int}/payment/paid")]
    public async Task<IActionResult> MarkPaidAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(adminUserId))
        {
            return Unauthorized();
        }

        try
        {
            var order = await _orderService.MarkPaidAsync(id, adminUserId, cancellationToken);
            return Ok(new { success = true, data = order });
        }
        catch (AdminOrderConflictException exception)
        {
            return Conflict(Error(exception.Code, exception.Message));
        }
        catch (AdminOrderNotFoundException)
        {
            return OrderNotFound();
        }
    }

    private static int ParsePositiveInteger(string? value, int fallback) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && parsed > 0
            ? parsed
            : fallback;

    private IActionResult OrderNotFound() =>
        NotFound(Error("ORDER_NOT_FOUND", "Order not found."));

    private static object Error(string code, string message) =>
        new { success = false, error = new { code, message } };
}
