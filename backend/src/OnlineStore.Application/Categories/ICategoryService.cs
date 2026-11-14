using OnlineStore.Application.Products;

namespace OnlineStore.Application.Categories;

public interface ICategoryService
{
    Task<IReadOnlyList<CategorySummaryDto>> GetActiveCategoriesAsync(CancellationToken cancellationToken);
}
