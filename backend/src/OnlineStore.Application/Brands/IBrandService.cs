using OnlineStore.Application.Products;

namespace OnlineStore.Application.Brands;

public interface IBrandService
{
    Task<IReadOnlyList<BrandSummaryDto>> GetBrandsAsync(CancellationToken cancellationToken);
}
