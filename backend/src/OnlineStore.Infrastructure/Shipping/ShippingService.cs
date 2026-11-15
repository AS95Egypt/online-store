using Microsoft.EntityFrameworkCore;
using OnlineStore.Application.Shipping;
using OnlineStore.Infrastructure.Persistence;

namespace OnlineStore.Infrastructure.Shipping;

public sealed class ShippingService : IShippingService
{
    private readonly AppDbContext _dbContext;

    public ShippingService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<GovernorateDto>> GetOptionsAsync(
        CancellationToken cancellationToken = default) =>
        await _dbContext.Governorates
            .AsNoTracking()
            .OrderBy(governorate => governorate.Name)
            .Select(governorate => new GovernorateDto(
                governorate.Id,
                governorate.Name,
                governorate.ShippingFee))
            .ToListAsync(cancellationToken);
}
