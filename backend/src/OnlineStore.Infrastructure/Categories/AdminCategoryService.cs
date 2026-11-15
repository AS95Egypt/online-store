using Microsoft.EntityFrameworkCore;
using OnlineStore.Application.Catalogs;
using OnlineStore.Application.Categories;
using OnlineStore.Domain.Entities;
using OnlineStore.Infrastructure.Persistence;

namespace OnlineStore.Infrastructure.Categories;

public sealed class AdminCategoryService : IAdminCategoryService
{
    private readonly AppDbContext _dbContext;

    public AdminCategoryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AdminCategoryDto>> GetAllAsync(CancellationToken cancellationToken) =>
        await _dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.NameEn)
            .Select(category => new AdminCategoryDto(
                category.Id,
                category.NameAr,
                category.NameEn,
                category.DisplayImageUrl,
                category.IsActive))
            .ToListAsync(cancellationToken);

    public async Task<AdminCategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        await _dbContext.Categories
            .AsNoTracking()
            .Where(category => category.Id == id)
            .Select(category => new AdminCategoryDto(
                category.Id,
                category.NameAr,
                category.NameEn,
                category.DisplayImageUrl,
                category.IsActive))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<AdminCategoryDto> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var (nameAr, nameEn) = ValidateNames(request.NameAr, request.NameEn);
        var imageUrl = NormalizeUrl(request.DisplayImageUrl);
        await EnsureNamesAreUniqueAsync(nameAr, nameEn, null, cancellationToken);

        var category = new Category
        {
            NameAr = nameAr,
            NameEn = nameEn,
            DisplayImageUrl = imageUrl,
            IsActive = true,
        };
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(category);
    }

    public async Task<AdminCategoryDto?> UpdateAsync(
        int id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
        if (category is null)
        {
            return null;
        }

        var (nameAr, nameEn) = ValidateNames(request.NameAr, request.NameEn);
        var imageUrl = NormalizeUrl(request.DisplayImageUrl);
        await EnsureNamesAreUniqueAsync(nameAr, nameEn, id, cancellationToken);
        category.NameAr = nameAr;
        category.NameEn = nameEn;
        category.DisplayImageUrl = imageUrl;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(category);
    }

    public async Task<AdminCategoryDto?> SetStatusAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
        if (category is null)
        {
            return null;
        }

        category.IsActive = isActive;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(category);
    }

    private async Task EnsureNamesAreUniqueAsync(
        string nameAr,
        string nameEn,
        int? excludedId,
        CancellationToken cancellationToken)
    {
        IQueryable<Category> categories = _dbContext.Categories.AsNoTracking();
        if (excludedId is { } currentId)
        {
            categories = categories.Where(category => category.Id != currentId);
        }

        var duplicateArabic = await categories.AnyAsync(
            category => category.NameAr.ToLower() == nameAr.ToLower(),
            cancellationToken);
        var duplicateEnglish = await categories.AnyAsync(
            category => category.NameEn.ToLower() == nameEn.ToLower(),
            cancellationToken);
        if (!duplicateArabic && !duplicateEnglish)
        {
            return;
        }

        var errors = new Dictionary<string, string[]>();
        if (duplicateArabic)
        {
            errors[nameof(Category.NameAr)] = ["A category with this Arabic name already exists."];
        }
        if (duplicateEnglish)
        {
            errors[nameof(Category.NameEn)] = ["A category with this English name already exists."];
        }

        throw new CatalogValidationException("duplicate_category_name", "Category names must be unique.", errors);
    }

    private static (string NameAr, string NameEn) ValidateNames(string? nameAr, string? nameEn)
    {
        var errors = new Dictionary<string, string[]>();
        ValidateName(nameAr, nameof(Category.NameAr), errors);
        ValidateName(nameEn, nameof(Category.NameEn), errors);
        if (errors.Count > 0)
        {
            throw new CatalogValidationException("invalid_category", "Enter valid category names.", errors);
        }

        return (nameAr!.Trim(), nameEn!.Trim());
    }

    private static void ValidateName(string? value, string field, IDictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[field] = ["A name is required."];
        }
        else if (value.Trim().Length > 200)
        {
            errors[field] = ["Names must be 200 characters or fewer."];
        }
    }

    private static string? NormalizeUrl(string? value)
    {
        if (value?.Trim().Length > 2048)
        {
            throw new CatalogValidationException(
                "invalid_category",
                "Enter a valid category image URL.",
                new Dictionary<string, string[]>
                {
                    [nameof(CreateCategoryRequest.DisplayImageUrl)] = ["Image URLs must be 2048 characters or fewer."],
                });
        }

        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static AdminCategoryDto ToDto(Category category) =>
        new(category.Id, category.NameAr, category.NameEn, category.DisplayImageUrl, category.IsActive);
}
