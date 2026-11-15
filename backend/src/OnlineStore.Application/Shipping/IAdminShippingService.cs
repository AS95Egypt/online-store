namespace OnlineStore.Application.Shipping;

public interface IAdminShippingService
{
    Task<IReadOnlyList<AdminGovernorateDto>> GetGovernoratesAsync(
        CancellationToken cancellationToken = default);

    Task<AdminGovernorateDto?> UpdateFeeAsync(
        int governorateId,
        decimal shippingFee,
        CancellationToken cancellationToken = default);
}
