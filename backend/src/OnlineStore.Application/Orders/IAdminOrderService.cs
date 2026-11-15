namespace OnlineStore.Application.Orders;

public interface IAdminOrderService
{
    Task<AdminOrderListResultDto> GetOrdersAsync(
        string? status,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<AdminOrderDetailsDto?> GetOrderDetailsAsync(
        int orderId,
        CancellationToken cancellationToken = default);

    Task<AdminOrderDetailsDto> UpdateStatusAsync(
        int orderId,
        string targetStatus,
        string adminUserId,
        CancellationToken cancellationToken = default);

    Task<AdminOrderDetailsDto> CancelOrderAsync(
        int orderId,
        string adminUserId,
        CancellationToken cancellationToken = default);

    Task<AdminOrderDetailsDto> MarkPaidAsync(
        int orderId,
        string adminUserId,
        CancellationToken cancellationToken = default);
}
