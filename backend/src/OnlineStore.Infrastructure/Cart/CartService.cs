using Microsoft.EntityFrameworkCore;
using OnlineStore.Application.Cart;
using OnlineStore.Domain.Entities;
using OnlineStore.Infrastructure.Persistence;

namespace OnlineStore.Infrastructure.Carts;

public sealed class CartService : ICartService
{
    private readonly AppDbContext _dbContext;

    public CartService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CartDto> GetCartAsync(string userId, CancellationToken cancellationToken)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        return await BuildCartDtoAsync(cart, cancellationToken);
    }

    public async Task<CartOperationResult> AddItemAsync(
        string userId,
        AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Quantity < 1)
        {
            return CartOperationResult.Failure("invalid_quantity", "Quantity must be at least 1.");
        }

        var product = await _dbContext.Products
            .SingleOrDefaultAsync(candidate => candidate.Id == request.ProductId, cancellationToken);
        if (product is null)
        {
            return CartOperationResult.Failure("product_not_found", "Product not found.", 404);
        }

        var productError = ValidateProductCanBeAdded(product);
        if (productError is not null)
        {
            return productError;
        }

        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var existingItem = await _dbContext.CartItems
            .SingleOrDefaultAsync(
                item => item.CartId == cart.Id && item.ProductId == product.Id,
                cancellationToken);
        var finalQuantity = (long)(existingItem?.Quantity ?? 0) + request.Quantity;
        if (finalQuantity > product.Qty)
        {
            return CartOperationResult.Failure(
                "insufficient_stock",
                $"Only {product.Qty} item(s) are currently available.");
        }

        if (existingItem is null)
        {
            _dbContext.CartItems.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = product.Id,
                Quantity = request.Quantity,
            });
        }
        else
        {
            existingItem.Quantity = (int)finalQuantity;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return CartOperationResult.Success(await BuildCartDtoAsync(cart, cancellationToken));
    }

    public async Task<CartOperationResult> UpdateItemAsync(
        string userId,
        int productId,
        UpdateCartItemRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Quantity < 1)
        {
            return CartOperationResult.Failure("invalid_quantity", "Quantity must be at least 1.");
        }

        var cart = await _dbContext.Carts
            .SingleOrDefaultAsync(candidate => candidate.UserId == userId, cancellationToken);
        if (cart is null)
        {
            return CartOperationResult.Failure("cart_item_not_found", "Cart item not found.", 404);
        }

        var item = await _dbContext.CartItems
            .Include(candidate => candidate.Product)
            .SingleOrDefaultAsync(
                candidate => candidate.CartId == cart.Id && candidate.ProductId == productId,
                cancellationToken);
        if (item is null)
        {
            return CartOperationResult.Failure("cart_item_not_found", "Cart item not found.", 404);
        }

        var productError = ValidateProductCanBeAdded(item.Product);
        if (productError is not null)
        {
            return productError;
        }

        if (request.Quantity > item.Product.Qty)
        {
            return CartOperationResult.Failure(
                "insufficient_stock",
                $"Only {item.Product.Qty} item(s) are currently available.");
        }

        item.Quantity = request.Quantity;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return CartOperationResult.Success(await BuildCartDtoAsync(cart, cancellationToken));
    }

    public async Task<CartOperationResult> RemoveItemAsync(
        string userId,
        int productId,
        CancellationToken cancellationToken)
    {
        var cart = await _dbContext.Carts
            .SingleOrDefaultAsync(candidate => candidate.UserId == userId, cancellationToken);
        if (cart is null)
        {
            return CartOperationResult.Failure("cart_item_not_found", "Cart item not found.", 404);
        }

        var item = await _dbContext.CartItems
            .SingleOrDefaultAsync(
                candidate => candidate.CartId == cart.Id && candidate.ProductId == productId,
                cancellationToken);
        if (item is null)
        {
            return CartOperationResult.Failure("cart_item_not_found", "Cart item not found.", 404);
        }

        _dbContext.CartItems.Remove(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return CartOperationResult.Success(await BuildCartDtoAsync(cart, cancellationToken));
    }

    public async Task<CheckoutCalculationResultDto> CalculateCheckoutAsync(
        string userId,
        int? governorateId,
        CancellationToken cancellationToken)
    {
        var cart = await GetOrCreateCartAsync(userId, cancellationToken);
        var cartItems = await _dbContext.CartItems
            .AsNoTracking()
            .Where(item => item.CartId == cart.Id)
            .Include(item => item.Product)
            .OrderBy(item => item.Id)
            .ToListAsync(cancellationToken);

        var errors = CartValidation.ValidateCartItems(cartItems);
        if (errors.Count > 0)
        {
            return new CheckoutCalculationResultDto
            {
                IsValid = false,
                Errors = errors,
            };
        }

        var items = cartItems.Select(item => new CartItemDto
        {
            ProductId = item.ProductId,
            ItemNameEn = item.Product.ItemNameEn,
            PrimaryImageUrl = item.Product.PrimaryImageUrl,
            UnitPrice = item.Product.Price,
            Quantity = item.Quantity,
            LineTotal = item.Product.Price * item.Quantity,
        }).ToList();

        var result = new CheckoutCalculationResultDto
        {
            IsValid = true,
            Items = items,
            Subtotal = items.Sum(item => item.LineTotal),
            Errors = new List<CheckoutLineErrorDto>(),
        };

        if (governorateId is null)
        {
            return result;
        }

        var governorate = await _dbContext.Governorates
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == governorateId,
                cancellationToken);
        if (governorate is null)
        {
            result.IsValid = false;
            result.GovernorateError = "Select a valid governorate.";
            return result;
        }

        result.ShippingFee = governorate.ShippingFee;
        result.Total = result.Subtotal + governorate.ShippingFee;
        return result;
    }

    private async Task<Cart> GetOrCreateCartAsync(string userId, CancellationToken cancellationToken)
    {
        var cart = await _dbContext.Carts
            .SingleOrDefaultAsync(candidate => candidate.UserId == userId, cancellationToken);
        if (cart is not null)
        {
            return cart;
        }

        cart = new Cart { UserId = userId };
        _dbContext.Carts.Add(cart);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return cart;
    }

    private async Task<CartDto> BuildCartDtoAsync(Cart cart, CancellationToken cancellationToken)
    {
        var items = await _dbContext.CartItems
            .AsNoTracking()
            .Where(item => item.CartId == cart.Id)
            .OrderBy(item => item.Id)
            .Select(item => new CartItemDto
            {
                ProductId = item.ProductId,
                ItemNameEn = item.Product.ItemNameEn,
                PrimaryImageUrl = item.Product.PrimaryImageUrl,
                UnitPrice = item.Product.Price,
                Quantity = item.Quantity,
                LineTotal = item.Product.Price * item.Quantity,
            })
            .ToListAsync(cancellationToken);

        return new CartDto
        {
            CartId = cart.Id,
            Items = items,
            Subtotal = items.Sum(item => item.LineTotal),
        };
    }

    private static CartOperationResult? ValidateProductCanBeAdded(Product product)
    {
        if (!product.IsActive)
        {
            return CartOperationResult.Failure("product_unavailable", "This product is no longer available.");
        }

        if (product.StockStatus != ProductStockStatuses.InStock || product.Qty < 1)
        {
            return CartOperationResult.Failure("out_of_stock", "This product is out of stock.");
        }

        return null;
    }
}
