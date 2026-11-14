using Microsoft.EntityFrameworkCore;
using OnlineStore.Domain.Entities;
using OnlineStore.Application.Products;
using OnlineStore.Infrastructure.Persistence;

namespace OnlineStore.Infrastructure.Products;

public class ProductService : IProductService
{
    private readonly AppDbContext _dbContext;

    public ProductService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ProductListItemDto>> GetActiveProductsAsync(
        ProductListQuery query,
        CancellationToken cancellationToken)
    {
        var page = query.Page;
        var pageSize = query.PageSize;
        page = page < 1 ? ProductPaginationDefaults.Page : page;
        pageSize = pageSize < 1
            ? ProductPaginationDefaults.PageSize
            : Math.Min(pageSize, ProductPaginationDefaults.MaxPageSize);

        var activeProducts = _dbContext.Products
            .AsNoTracking()
            .Where(product => product.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLowerInvariant();
            activeProducts = activeProducts.Where(product =>
                EF.Functions.Like(product.ItemNameEn.ToLower(), $"%{term}%") ||
                EF.Functions.Like(product.ItemNameAr.ToLower(), $"%{term}%"));
        }

        if (query.CategoryId is { } categoryId)
        {
            activeProducts = activeProducts.Where(product => product.CategoryId == categoryId);
        }

        if (query.BrandIds is { Count: > 0 } brandIds)
        {
            activeProducts = activeProducts.Where(product => brandIds.Contains(product.BrandId));
        }

        if (query.MinPrice is { } minPrice)
        {
            activeProducts = activeProducts.Where(product => product.Price >= minPrice);
        }

        if (query.MaxPrice is { } maxPrice)
        {
            activeProducts = activeProducts.Where(product => product.Price <= maxPrice);
        }

        if (!string.IsNullOrWhiteSpace(query.StockStatus))
        {
            activeProducts = activeProducts.Where(product => product.StockStatus == query.StockStatus);
        }

        var total = await activeProducts.CountAsync(cancellationToken);
        var skip = (int)Math.Min(((long)page - 1) * pageSize, int.MaxValue);
        var orderedProducts = query.Sort switch
        {
            ProductSortOrder.PriceAsc => activeProducts.OrderBy(product => product.Price).ThenBy(product => product.Id),
            ProductSortOrder.PriceDesc => activeProducts.OrderByDescending(product => product.Price).ThenBy(product => product.Id),
            _ => activeProducts.OrderBy(product => product.Id),
        };
        var items = await orderedProducts
            .Include(product => product.Brand)
            .Include(product => product.Category)
            .Skip(skip)
            .Take(pageSize)
            .Select(product => new ProductListItemDto(
                product.Id,
                product.ItemNameAr,
                product.ItemNameEn,
                product.Price,
                product.Qty,
                product.StockStatus,
                product.PrimaryImageUrl,
                new BrandSummaryDto(
                    product.Brand.Id,
                    product.Brand.NameAr,
                    product.Brand.NameEn,
                    product.Brand.DisplayImageUrl),
                new CategorySummaryDto(
                    product.Category.Id,
                    product.Category.NameAr,
                    product.Category.NameEn,
                    product.Category.DisplayImageUrl)))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductListItemDto>(
            true,
            items,
            new ProductPageMeta(total, page, pageSize));
    }

    public async Task<ProductDetailsDto?> GetActiveProductByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.IsActive && product.Id == id)
            .Include(product => product.Brand)
            .Include(product => product.Category)
            .Select(product => new ProductDetailsDto(
                product.Id,
                product.ItemNameAr,
                product.ItemNameEn,
                product.Price,
                product.Qty,
                product.StockStatus,
                product.PrimaryImageUrl,
                product.Thumb1Url,
                product.Thumb2Url,
                product.Thumb3Url,
                product.Thumb4Url,
                product.DescriptionAr,
                product.DescriptionEn,
                new BrandSummaryDto(
                    product.Brand.Id,
                    product.Brand.NameAr,
                    product.Brand.NameEn,
                    product.Brand.DisplayImageUrl),
                new CategorySummaryDto(
                    product.Category.Id,
                    product.Category.NameAr,
                    product.Category.NameEn,
                    product.Category.DisplayImageUrl)))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
