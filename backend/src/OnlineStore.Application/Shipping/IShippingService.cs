namespace OnlineStore.Application.Shipping;

public interface IShippingService
{
    Task<IReadOnlyList<GovernorateDto>> GetOptionsAsync(CancellationToken cancellationToken = default);
}
