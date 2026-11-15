using Microsoft.EntityFrameworkCore;
using OnlineStore.Application.Brands;
using OnlineStore.Application.Catalogs;
using OnlineStore.Domain.Entities;
using OnlineStore.Infrastructure.Persistence;

namespace OnlineStore.Infrastructure.Brands;

public sealed class AdminBrandService : IAdminBrandService
{
    private readonly AppDbContext _dbContext;

    public AdminBrandService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AdminBrandDto>> GetAllAsync(CancellationToken cancellationToken) =>
        await _dbContext.Brands
            .AsNoTracking()
            .OrderBy(brand => brand.NameEn)
            .Select(brand => new AdminBrandDto(
                brand.Id,
                brand.NameAr,
                brand.NameEn,
                brand.DisplayImageUrl))
            .ToListAsync(cancellationToken);

    public async Task<AdminBrandDto?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        await _dbContext.Brands
            .AsNoTracking()
            .Where(brand => brand.Id == id)
            .Select(brand => new AdminBrandDto(
                brand.Id,
                brand.NameAr,
                brand.NameEn,
                brand.DisplayImageUrl))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<AdminBrandDto> CreateAsync(
        CreateBrandRequest request,
        CancellationToken cancellationToken)
    {
        var (nameAr, nameEn) = ValidateNames(request.NameAr, request.NameEn);
        var imageUrl = NormalizeUrl(request.DisplayImageUrl);
        await EnsureNamesAreUniqueAsync(nameAr, nameEn, null, cancellationToken);
        var brand = new Brand
        {
            NameAr = nameAr,
            NameEn = nameEn,
            DisplayImageUrl = imageUrl,
        };
        _dbContext.Brands.Add(brand);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(brand);
    }

    public async Task<AdminBrandDto?> UpdateAsync(
        int id,
        UpdateBrandRequest request,
        CancellationToken cancellationToken)
    {
        var brand = await _dbContext.Brands
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
        if (brand is null)
        {
            return null;
        }

        var (nameAr, nameEn) = ValidateNames(request.NameAr, request.NameEn);
        var imageUrl = NormalizeUrl(request.DisplayImageUrl);
        await EnsureNamesAreUniqueAsync(nameAr, nameEn, id, cancellationToken);
        brand.NameAr = nameAr;
        brand.NameEn = nameEn;
        brand.DisplayImageUrl = imageUrl;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(brand);
    }

    private async Task EnsureNamesAreUniqueAsync(
        string nameAr,
        string nameEn,
        int? excludedId,
        CancellationToken cancellationToken)
    {
        IQueryable<Brand> brands = _dbContext.Brands.AsNoTracking();
        if (excludedId is { } currentId)
        {
            brands = brands.Where(brand => brand.Id != currentId);
        }

        var duplicateArabic = await brands.AnyAsync(
            brand => brand.NameAr.ToLower() == nameAr.ToLower(),
            cancellationToken);
        var duplicateEnglish = await brands.AnyAsync(
            brand => brand.NameEn.ToLower() == nameEn.ToLower(),
            cancellationToken);
        if (!duplicateArabic && !duplicateEnglish)
        {
            return;
        }

        var errors = new Dictionary<string, string[]>();
        if (duplicateArabic)
        {
            errors[nameof(Brand.NameAr)] = ["A brand with this Arabic name already exists."];
        }
        if (duplicateEnglish)
        {
            errors[nameof(Brand.NameEn)] = ["A brand with this English name already exists."];
        }

        throw new CatalogValidationException("duplicate_brand_name", "Brand names must be unique.", errors);
    }

    private static (string NameAr, string NameEn) ValidateNames(string? nameAr, string? nameEn)
    {
        var errors = new Dictionary<string, string[]>();
        ValidateName(nameAr, nameof(Brand.NameAr), errors);
        ValidateName(nameEn, nameof(Brand.NameEn), errors);
        if (errors.Count > 0)
        {
            throw new CatalogValidationException("invalid_brand", "Enter valid brand names.", errors);
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
                "invalid_brand",
                "Enter a valid brand image URL.",
                new Dictionary<string, string[]>
                {
                    [nameof(CreateBrandRequest.DisplayImageUrl)] = ["Image URLs must be 2048 characters or fewer."],
                });
        }

        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static AdminBrandDto ToDto(Brand brand) =>
        new(brand.Id, brand.NameAr, brand.NameEn, brand.DisplayImageUrl);
}
