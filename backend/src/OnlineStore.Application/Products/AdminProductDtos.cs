namespace OnlineStore.Application.Products;

public sealed record AdminProductListItemDto(
    int Id,
    string Sku,
    string NameAr,
    string NameEn,
    decimal Price,
    int Qty,
    bool IsActive,
    string? PrimaryImageUrl,
    BrandSummaryDto Brand,
    CategorySummaryDto Category);

public sealed record AdminProductDetailsDto(
    int Id,
    string Sku,
    string NameAr,
    string NameEn,
    decimal Price,
    int Qty,
    bool IsActive,
    string? DescriptionAr,
    string? DescriptionEn,
    string? PrimaryImageUrl,
    string? Thumb1Url,
    string? Thumb2Url,
    string? Thumb3Url,
    string? Thumb4Url,
    BrandSummaryDto Brand,
    CategorySummaryDto Category);

public sealed record AdminProductListQuery(int Page, int PageSize, string? Search);

public sealed record CreateProductRequest(
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

public sealed record UpdateProductRequest(
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

public sealed record UpdateProductStatusRequest(bool IsActive);
