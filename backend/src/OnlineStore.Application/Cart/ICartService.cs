namespace OnlineStore.Application.Cart;

public interface ICartService
{
    Task<CartDto> GetCartAsync(string userId, CancellationToken cancellationToken);

    Task<CartOperationResult> AddItemAsync(
        string userId,
        AddCartItemRequest request,
        CancellationToken cancellationToken);

    Task<CartOperationResult> UpdateItemAsync(
        string userId,
        int productId,
        UpdateCartItemRequest request,
        CancellationToken cancellationToken);

    Task<CartOperationResult> RemoveItemAsync(
        string userId,
        int productId,
        CancellationToken cancellationToken);

    Task<CheckoutCalculationResultDto> CalculateCheckoutAsync(
        string userId,
        int? governorateId,
        CancellationToken cancellationToken);
}
