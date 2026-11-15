namespace OnlineStore.Application.Cart;

public sealed class CartItemDto
{
    public int ProductId { get; set; }
    public string ItemNameEn { get; set; } = string.Empty;
    public string? PrimaryImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class CartDto
{
    public int CartId { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
}

public sealed class AddCartItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public sealed class UpdateCartItemRequest
{
    public int Quantity { get; set; }
}

public sealed class CheckoutLineErrorDto
{
    public int ProductId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public sealed class CheckoutCalculationResultDto
{
    public bool IsValid { get; set; }
    public List<CartItemDto>? Items { get; set; }
    public decimal? Subtotal { get; set; }
    public decimal? ShippingFee { get; set; }
    public decimal? Total { get; set; }
    public string? GovernorateError { get; set; }
    public List<CheckoutLineErrorDto>? Errors { get; set; }
}

public sealed record CartOperationResult(
    CartDto? Cart = null,
    string? ErrorCode = null,
    string? ErrorMessage = null,
    int StatusCode = 400)
{
    public bool Succeeded => Cart is not null && ErrorCode is null;

    public static CartOperationResult Success(CartDto cart) => new(Cart: cart);

    public static CartOperationResult Failure(string code, string message, int statusCode = 400) =>
        new(ErrorCode: code, ErrorMessage: message, StatusCode: statusCode);
}
