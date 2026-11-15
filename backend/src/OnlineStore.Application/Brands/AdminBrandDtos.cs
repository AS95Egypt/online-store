namespace OnlineStore.Application.Brands;

public sealed record AdminBrandDto(int Id, string NameAr, string NameEn, string? DisplayImageUrl);

public sealed record CreateBrandRequest(string NameAr, string NameEn, string? DisplayImageUrl);

public sealed record UpdateBrandRequest(string NameAr, string NameEn, string? DisplayImageUrl);
