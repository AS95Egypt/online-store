namespace OnlineStore.Application.Products;

public record ProductListItemDto(
    int Id,
    string NameAr,
    string NameEn,
    decimal Price,
    int Qty,
    string StockStatus,
    string? PrimaryImageUrl,
    BrandSummaryDto Brand,
    CategorySummaryDto Category);

public record ProductDetailsDto(
    int Id,
    string NameAr,
    string NameEn,
    decimal Price,
    int Qty,
    string StockStatus,
    string? PrimaryImageUrl,
    string? Thumb1Url,
    string? Thumb2Url,
    string? Thumb3Url,
    string? Thumb4Url,
    string? DescriptionAr,
    string? DescriptionEn,
    BrandSummaryDto Brand,
    CategorySummaryDto Category);

public enum ProductSortOrder
{
    None,
    PriceAsc,
    PriceDesc,
}

public record ProductListQuery(
    int Page,
    int PageSize,
    string? Search,
    int? CategoryId,
    IReadOnlyList<int>? BrandIds,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? StockStatus,
    ProductSortOrder Sort);

public record BrandSummaryDto(int Id, string NameAr, string NameEn, string? DisplayImageUrl);

public record CategorySummaryDto(int Id, string NameAr, string NameEn, string? DisplayImageUrl);

public record ProductPageMeta(int Total, int Page, int PageSize);

public record PagedResult<T>(bool Success, IReadOnlyList<T> Data, ProductPageMeta Meta);
