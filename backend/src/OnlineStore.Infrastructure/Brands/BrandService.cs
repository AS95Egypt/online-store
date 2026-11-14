using Microsoft.EntityFrameworkCore;
using OnlineStore.Application.Brands;
using OnlineStore.Application.Products;
using OnlineStore.Infrastructure.Persistence;

namespace OnlineStore.Infrastructure.Brands;

public sealed class BrandService : IBrandService
{
    private readonly AppDbContext _dbContext;

    public BrandService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<BrandSummaryDto>> GetBrandsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Brands
            .AsNoTracking()
            .OrderBy(brand => brand.NameEn)
            .Select(brand => new BrandSummaryDto(
                brand.Id,
                brand.NameAr,
                brand.NameEn,
                brand.DisplayImageUrl))
            .ToListAsync(cancellationToken);
    }
}
