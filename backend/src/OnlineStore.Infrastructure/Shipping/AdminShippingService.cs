using Microsoft.EntityFrameworkCore;
using OnlineStore.Application.Shipping;
using OnlineStore.Infrastructure.Persistence;

namespace OnlineStore.Infrastructure.Shipping;

public sealed class AdminShippingService : IAdminShippingService
{
    private readonly AppDbContext _dbContext;

    public AdminShippingService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AdminGovernorateDto>> GetGovernoratesAsync(
        CancellationToken cancellationToken = default) =>
        await _dbContext.Governorates
            .AsNoTracking()
            .OrderBy(governorate => governorate.Name)
            .Select(governorate => new AdminGovernorateDto(
                governorate.Id,
                governorate.Name,
                governorate.ShippingFee))
            .ToListAsync(cancellationToken);

    public async Task<AdminGovernorateDto?> UpdateFeeAsync(
        int governorateId,
        decimal shippingFee,
        CancellationToken cancellationToken = default)
    {
        if (shippingFee < 0 || shippingFee > 9999999999999999m)
        {
            throw new ShippingFeeValidationException(
                "INVALID_SHIPPING_FEE",
                "Shipping fee must be between 0 and 9999999999999999.",
                new Dictionary<string, string[]>
                {
                    [nameof(UpdateGovernorateFeeRequest.ShippingFee)] =
                    ["Enter a non-negative shipping fee within the supported range."],
                });
        }

        var governorate = await _dbContext.Governorates
            .SingleOrDefaultAsync(candidate => candidate.Id == governorateId, cancellationToken);
        if (governorate is null)
        {
            return null;
        }

        governorate.ShippingFee = shippingFee;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new AdminGovernorateDto(governorate.Id, governorate.Name, governorate.ShippingFee);
    }
}
