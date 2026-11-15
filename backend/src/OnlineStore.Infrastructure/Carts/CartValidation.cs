using OnlineStore.Application.Cart;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Carts;

internal static class CartValidation
{
    public static List<CheckoutLineErrorDto> ValidateCartItems(IEnumerable<CartItem> cartItems)
    {
        var errors = new List<CheckoutLineErrorDto>();
        foreach (var item in cartItems)
        {
            var reason = GetProductError(item.Product, item.Quantity);

            if (reason is not null)
            {
                errors.Add(new CheckoutLineErrorDto { ProductId = item.ProductId, Reason = reason });
            }
        }

        return errors;
    }

    public static string? GetProductError(Product product, int requestedQuantity)
    {
        if (!product.IsActive)
        {
            return "This product is no longer available.";
        }

        if (product.StockStatus != ProductStockStatuses.InStock || product.Qty < 1)
        {
            return "This product is out of stock.";
        }

        if (requestedQuantity > product.Qty)
        {
            return $"Only {product.Qty} item(s) are currently available.";
        }

        return null;
    }
}
