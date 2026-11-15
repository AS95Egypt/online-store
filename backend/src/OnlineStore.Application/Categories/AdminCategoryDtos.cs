namespace OnlineStore.Application.Categories;

public sealed record AdminCategoryDto(
    int Id,
    string NameAr,
    string NameEn,
    string? DisplayImageUrl,
    bool IsActive);

public sealed record CreateCategoryRequest(string NameAr, string NameEn, string? DisplayImageUrl);

public sealed record UpdateCategoryRequest(string NameAr, string NameEn, string? DisplayImageUrl);

public sealed record UpdateCategoryStatusRequest(bool IsActive);
