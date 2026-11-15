using Microsoft.EntityFrameworkCore;
using OnlineStore.Application.Catalogs;
using OnlineStore.Application.Products;
using OnlineStore.Domain.Entities;
using OnlineStore.Infrastructure.Persistence;

namespace OnlineStore.Infrastructure.Products;

public sealed class AdminProductService : IAdminProductService
{
    private readonly AppDbContext _dbContext;

    public AdminProductService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<AdminProductListItemDto>> GetPagedAsync(
        AdminProductListQuery query,
        CancellationToken cancellationToken)
    {
        var page = query.Page < 1 ? ProductPaginationDefaults.Page : query.Page;
        var pageSize = query.PageSize < 1
            ? ProductPaginationDefaults.PageSize
            : Math.Min(query.PageSize, ProductPaginationDefaults.MaxPageSize);
        var products = _dbContext.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLowerInvariant();
            products = products.Where(product =>
                EF.Functions.Like(product.ItemNameEn.ToLower(), $"%{term}%") ||
                EF.Functions.Like(product.ItemNameAr.ToLower(), $"%{term}%") ||
                EF.Functions.Like(product.Sku.ToLower(), $"%{term}%"));
        }

        var total = await products.CountAsync(cancellationToken);
        var skip = (int)Math.Min(((long)page - 1) * pageSize, int.MaxValue);
        var items = await products
            .OrderBy(product => product.Id)
            .Skip(skip)
            .Take(pageSize)
            .Select(product => new AdminProductListItemDto(
                product.Id,
                product.Sku,
                product.ItemNameAr,
                product.ItemNameEn,
                product.Price,
                product.Qty,
                product.IsActive,
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
        return new PagedResult<AdminProductListItemDto>(
            true,
            items,
            new ProductPageMeta(total, page, pageSize));
    }

    public async Task<AdminProductDetailsDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.Id == id)
            .Include(product => product.Brand)
            .Include(product => product.Category)
            .SingleOrDefaultAsync(cancellationToken);
        return product is null ? null : ToDetails(product);
    }

    public async Task<AdminProductDetailsDto> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var values = Validate(request);
        await EnsureReferencesAreValidAsync(values.CategoryId, values.BrandId, cancellationToken);
        var sku = await GenerateSkuAsync(cancellationToken);
        var product = new Product
        {
            Sku = sku,
            ItemNameAr = values.NameAr,
            ItemNameEn = values.NameEn,
            DescriptionAr = values.DescriptionAr,
            DescriptionEn = values.DescriptionEn,
            Price = values.Price,
            Qty = values.Qty,
            CategoryId = values.CategoryId,
            BrandId = values.BrandId,
            StockStatus = values.Qty > 0 ? ProductStockStatuses.InStock : ProductStockStatuses.OutOfStock,
            IsActive = true,
            PrimaryImageUrl = values.PrimaryImageUrl,
            Thumb1Url = values.Thumb1Url,
            Thumb2Url = values.Thumb2Url,
            Thumb3Url = values.Thumb3Url,
            Thumb4Url = values.Thumb4Url,
        };
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetRequiredByIdAsync(product.Id, cancellationToken);
    }

    public async Task<AdminProductDetailsDto?> UpdateAsync(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
        if (product is null)
        {
            return null;
        }

        var values = Validate(request);
        await EnsureReferencesAreValidAsync(values.CategoryId, values.BrandId, cancellationToken);
        product.ItemNameAr = values.NameAr;
        product.ItemNameEn = values.NameEn;
        product.DescriptionAr = values.DescriptionAr;
        product.DescriptionEn = values.DescriptionEn;
        product.Price = values.Price;
        product.Qty = values.Qty;
        product.CategoryId = values.CategoryId;
        product.BrandId = values.BrandId;
        product.StockStatus = values.Qty > 0 ? ProductStockStatuses.InStock : ProductStockStatuses.OutOfStock;
        product.PrimaryImageUrl = values.PrimaryImageUrl;
        product.Thumb1Url = values.Thumb1Url;
        product.Thumb2Url = values.Thumb2Url;
        product.Thumb3Url = values.Thumb3Url;
        product.Thumb4Url = values.Thumb4Url;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<AdminProductDetailsDto?> SetStatusAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
        if (product is null)
        {
            return null;
        }

        product.IsActive = isActive;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    private async Task EnsureReferencesAreValidAsync(
        int categoryId,
        int brandId,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        var category = await _dbContext.Categories
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == categoryId, cancellationToken);
        if (category is null)
        {
            errors[nameof(CreateProductRequest.CategoryId)] = ["Select an existing category."];
        }
        else if (!category.IsActive)
        {
            errors[nameof(CreateProductRequest.CategoryId)] = ["Select an active category."];
        }

        if (!await _dbContext.Brands.AnyAsync(brand => brand.Id == brandId, cancellationToken))
        {
            errors[nameof(CreateProductRequest.BrandId)] = ["Select an existing brand."];
        }

        if (errors.Count > 0)
        {
            throw new CatalogValidationException(
                "invalid_product_reference",
                "Select a valid category and brand.",
                errors);
        }
    }

    private async Task<string> GenerateSkuAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidate = $"SKU-{Guid.NewGuid():N}"[..12].ToUpperInvariant();
            if (!await _dbContext.Products.AnyAsync(product => product.Sku == candidate, cancellationToken))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("A unique product SKU could not be generated.");
    }

    private async Task<AdminProductDetailsDto> GetRequiredByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await GetByIdAsync(id, cancellationToken);
        return product ?? throw new InvalidOperationException("The saved product could not be loaded.");
    }

    private static ProductValues Validate(CreateProductRequest request) =>
        ValidateValues(
            request.NameAr,
            request.NameEn,
            request.DescriptionAr,
            request.DescriptionEn,
            request.Price,
            request.Qty,
            request.CategoryId,
            request.BrandId,
            request.PrimaryImageUrl,
            request.Thumb1Url,
            request.Thumb2Url,
            request.Thumb3Url,
            request.Thumb4Url);

    private static ProductValues Validate(UpdateProductRequest request) =>
        ValidateValues(
            request.NameAr,
            request.NameEn,
            request.DescriptionAr,
            request.DescriptionEn,
            request.Price,
            request.Qty,
            request.CategoryId,
            request.BrandId,
            request.PrimaryImageUrl,
            request.Thumb1Url,
            request.Thumb2Url,
            request.Thumb3Url,
            request.Thumb4Url);

    private static ProductValues ValidateValues(
        string? nameAr,
        string? nameEn,
        string? descriptionAr,
        string? descriptionEn,
        decimal price,
        int qty,
        int categoryId,
        int brandId,
        string? primaryImageUrl,
        string? thumb1Url,
        string? thumb2Url,
        string? thumb3Url,
        string? thumb4Url)
    {
        var errors = new Dictionary<string, string[]>();
        ValidateRequired(nameAr, nameof(CreateProductRequest.NameAr), 250, errors);
        ValidateRequired(nameEn, nameof(CreateProductRequest.NameEn), 250, errors);
        ValidateOptional(descriptionAr, nameof(CreateProductRequest.DescriptionAr), 4000, errors);
        ValidateOptional(descriptionEn, nameof(CreateProductRequest.DescriptionEn), 4000, errors);
        ValidateImage(primaryImageUrl, nameof(CreateProductRequest.PrimaryImageUrl), errors);
        ValidateImage(thumb1Url, nameof(CreateProductRequest.Thumb1Url), errors);
        ValidateImage(thumb2Url, nameof(CreateProductRequest.Thumb2Url), errors);
        ValidateImage(thumb3Url, nameof(CreateProductRequest.Thumb3Url), errors);
        ValidateImage(thumb4Url, nameof(CreateProductRequest.Thumb4Url), errors);
        if (price <= 0)
        {
            errors[nameof(CreateProductRequest.Price)] = ["Price must be greater than zero."];
        }
        if (qty < 0)
        {
            errors[nameof(CreateProductRequest.Qty)] = ["Quantity cannot be negative."];
        }
        if (categoryId <= 0)
        {
            errors[nameof(CreateProductRequest.CategoryId)] = ["Select a valid category."];
        }
        if (brandId <= 0)
        {
            errors[nameof(CreateProductRequest.BrandId)] = ["Select a valid brand."];
        }
        if (string.IsNullOrWhiteSpace(primaryImageUrl) &&
            string.IsNullOrWhiteSpace(thumb1Url) &&
            string.IsNullOrWhiteSpace(thumb2Url) &&
            string.IsNullOrWhiteSpace(thumb3Url) &&
            string.IsNullOrWhiteSpace(thumb4Url))
        {
            errors[nameof(CreateProductRequest.PrimaryImageUrl)] = ["Add at least one image URL and choose a main image."];
        }
        else if (string.IsNullOrWhiteSpace(primaryImageUrl))
        {
            errors[nameof(CreateProductRequest.PrimaryImageUrl)] = ["A main image URL is required."];
        }

        if (errors.Count > 0)
        {
            throw new CatalogValidationException("invalid_product", "Review the product fields and try again.", errors);
        }

        return new ProductValues(
            nameAr!.Trim(),
            nameEn!.Trim(),
            NormalizeText(descriptionAr),
            NormalizeText(descriptionEn),
            price,
            qty,
            categoryId,
            brandId,
            NormalizeText(primaryImageUrl),
            NormalizeText(thumb1Url),
            NormalizeText(thumb2Url),
            NormalizeText(thumb3Url),
            NormalizeText(thumb4Url));
    }

    private static void ValidateRequired(
        string? value,
        string field,
        int maxLength,
        IDictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[field] = ["This field is required."];
        }
        else if (value.Trim().Length > maxLength)
        {
            errors[field] = [$"Must be {maxLength} characters or fewer."];
        }
    }

    private static void ValidateOptional(
        string? value,
        string field,
        int maxLength,
        IDictionary<string, string[]> errors)
    {
        if (value?.Trim().Length > maxLength)
        {
            errors[field] = [$"Must be {maxLength} characters or fewer."];
        }
    }

    private static void ValidateImage(string? value, string field, IDictionary<string, string[]> errors) =>
        ValidateOptional(value, field, 2048, errors);

    private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static AdminProductDetailsDto ToDetails(Product product) =>
        new(
            product.Id,
            product.Sku,
            product.ItemNameAr,
            product.ItemNameEn,
            product.Price,
            product.Qty,
            product.IsActive,
            product.DescriptionAr,
            product.DescriptionEn,
            product.PrimaryImageUrl,
            product.Thumb1Url,
            product.Thumb2Url,
            product.Thumb3Url,
            product.Thumb4Url,
            new BrandSummaryDto(
                product.Brand.Id,
                product.Brand.NameAr,
                product.Brand.NameEn,
                product.Brand.DisplayImageUrl),
            new CategorySummaryDto(
                product.Category.Id,
                product.Category.NameAr,
                product.Category.NameEn,
                product.Category.DisplayImageUrl));

    private sealed record ProductValues(
        string NameAr,
        string NameEn,
        string? DescriptionAr,
        string? DescriptionEn,
        decimal Price,
        int Qty,
        int CategoryId,
        int BrandId,
        string? PrimaryImageUrl,
        string? Thumb1Url,
        string? Thumb2Url,
        string? Thumb3Url,
        string? Thumb4Url);
}
