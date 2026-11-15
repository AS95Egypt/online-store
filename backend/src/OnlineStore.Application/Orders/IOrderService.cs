namespace OnlineStore.Application.Orders;

public interface IOrderService
{
    Task<OrderDetailsDto> CreateOrderAsync(
        string userId,
        CreateOrderRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderSummaryDto>> GetOrdersAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<OrderDetailsDto?> GetOrderDetailsAsync(
        string userId,
        string orderNumber,
        CancellationToken cancellationToken = default);

    Task<OrderDetailsDto?> CancelOrderAsync(
        string userId,
        string orderNumber,
        CancellationToken cancellationToken = default);
}
