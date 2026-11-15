using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStore.Application.Orders;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize(Roles = UserRoles.Customer)]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpPost]
    public async Task<IActionResult> CreateOrderAsync(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var order = await _orderService.CreateOrderAsync(userId, request, cancellationToken);
            return CreatedAtRoute(
                nameof(GetOrderDetailsAsync),
                new { orderNumber = order.OrderNumber },
                order);
        }
        catch (OrderValidationException exception)
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

    [HttpGet]
    public async Task<IActionResult> GetOrdersAsync(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        return Ok(await _orderService.GetOrdersAsync(userId, cancellationToken));
    }

    [HttpGet("{orderNumber}", Name = nameof(GetOrderDetailsAsync))]
    public async Task<IActionResult> GetOrderDetailsAsync(
        string orderNumber,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var order = await _orderService.GetOrderDetailsAsync(userId, orderNumber, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost("{orderNumber}/cancel")]
    public async Task<IActionResult> CancelOrderAsync(
        string orderNumber,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var order = await _orderService.CancelOrderAsync(userId, orderNumber, cancellationToken);
            return order is null ? NotFound() : Ok(order);
        }
        catch (OrderNotCancellableException exception)
        {
            return Conflict(new
            {
                success = false,
                error = new
                {
                    code = exception.Code,
                    message = exception.Message,
                },
            });
        }
    }
}
