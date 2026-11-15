using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStore.Application.Cart;

namespace OnlineStore.Api.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public sealed class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet]
    public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        return Ok(await _cartService.GetCartAsync(userId, cancellationToken));
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(
        [FromBody] AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        return ToActionResult(await _cartService.AddItemAsync(userId, request, cancellationToken));
    }

    [HttpPut("items/{productId:int}")]
    public async Task<IActionResult> UpdateItem(
        int productId,
        [FromBody] UpdateCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        return ToActionResult(
            await _cartService.UpdateItemAsync(userId, productId, request, cancellationToken));
    }

    [HttpDelete("items/{productId:int}")]
    public async Task<IActionResult> RemoveItem(int productId, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        return ToActionResult(await _cartService.RemoveItemAsync(userId, productId, cancellationToken));
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(
        [FromQuery] int? governorateId,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        return Ok(await _cartService.CalculateCheckoutAsync(userId, governorateId, cancellationToken));
    }

    private IActionResult ToActionResult(CartOperationResult result)
    {
        if (result.Succeeded)
        {
            return Ok(result.Cart);
        }

        var error = new
        {
            success = false,
            error = new
            {
                code = result.ErrorCode ?? "cart_operation_failed",
                message = result.ErrorMessage ?? "The cart could not be updated.",
            },
        };

        return result.StatusCode == StatusCodes.Status404NotFound
            ? NotFound(error)
            : BadRequest(error);
    }
}
