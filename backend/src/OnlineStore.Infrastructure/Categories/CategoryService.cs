using Microsoft.EntityFrameworkCore;
using OnlineStore.Application.Categories;
using OnlineStore.Application.Products;
using OnlineStore.Infrastructure.Persistence;

namespace OnlineStore.Infrastructure.Categories;

public sealed class CategoryService : ICategoryService
{
    private readonly AppDbContext _dbContext;

    public CategoryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CategorySummaryDto>> GetActiveCategoriesAsync(
        CancellationToken cancellationToken)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .OrderBy(category => category.NameEn)
            .Select(category => new CategorySummaryDto(
                category.Id,
                category.NameAr,
                category.NameEn,
                category.DisplayImageUrl))
            .ToListAsync(cancellationToken);
    }
}
