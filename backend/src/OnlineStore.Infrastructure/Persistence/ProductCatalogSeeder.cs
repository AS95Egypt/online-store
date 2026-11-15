using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Persistence;

public sealed class ProductCatalogSeeder
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ProductCatalogSeeder> _logger;

    public ProductCatalogSeeder(AppDbContext dbContext, ILogger<ProductCatalogSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var brand = await _dbContext.Brands
            .SingleOrDefaultAsync(x => x.NameEn == "Home Tech", cancellationToken);
        if (brand is null)
        {
            brand = new Brand
            {
                NameAr = "هوم تك",
                NameEn = "Home Tech",
            };
            _dbContext.Brands.Add(brand);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded product catalog brand.");
        }

        var category = await _dbContext.Categories
            .SingleOrDefaultAsync(x => x.NameEn == "Accessories", cancellationToken);
        if (category is null)
        {
            category = new Category
            {
                NameAr = "إكسسوارات",
                NameEn = "Accessories",
            };
            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seeded product catalog category.");
        }

        if (await _dbContext.Products.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Product catalog already contains products; skipping sample products.");
            return;
        }

        _dbContext.Products.AddRange(
            new Product
            {
                Sku = "SKU-0001",
                ItemNameAr = "سماعات لاسلكية",
                ItemNameEn = "Wireless Headphones",
                CategoryId = category.Id,
                BrandId = brand.Id,
                Price = 1250m,
                Qty = 12,
                StockStatus = ProductStockStatuses.InStock,
                IsActive = true,
                DescriptionEn = "Comfortable wireless headphones for everyday listening.",
            },
            new Product
            {
                Sku = "SKU-0002",
                ItemNameAr = "ماوس لاسلكي",
                ItemNameEn = "Wireless Mouse",
                CategoryId = category.Id,
                BrandId = brand.Id,
                Price = 480m,
                Qty = 0,
                StockStatus = ProductStockStatuses.OutOfStock,
                IsActive = true,
                DescriptionEn = "A compact wireless mouse.",
            },
            new Product
            {
                Sku = "SKU-0003",
                ItemNameAr = "لوحة مفاتيح ميكانيكية",
                ItemNameEn = "Mechanical Keyboard",
                CategoryId = category.Id,
                BrandId = brand.Id,
                Price = 1899.50m,
                Qty = 5,
                StockStatus = ProductStockStatuses.InStock,
                IsActive = true,
                DescriptionEn = "Mechanical keyboard with a clean, durable design.",
            },
            new Product
            {
                Sku = "SKU-0004",
                ItemNameAr = "حامل هاتف",
                ItemNameEn = "Phone Stand",
                CategoryId = category.Id,
                BrandId = brand.Id,
                Price = 210m,
                Qty = 8,
                StockStatus = ProductStockStatuses.InStock,
                IsActive = false,
                DescriptionEn = "Inactive sample product used to verify public filtering.",
            });

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded sample product catalog entries.");
    }
}
