namespace OnlineStore.Domain.Entities;

public static class ProductStockStatuses
{
    public const string InStock = "InStock";
    public const string OutOfStock = "OutOfStock";
}

public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string ItemNameAr { get; set; } = string.Empty;
    public string ItemNameEn { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int BrandId { get; set; }
    public decimal Price { get; set; }
    public int Qty { get; set; }
    public string StockStatus { get; set; } = ProductStockStatuses.InStock;
    public bool IsActive { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public string? Thumb1Url { get; set; }
    public string? Thumb2Url { get; set; }
    public string? Thumb3Url { get; set; }
    public string? Thumb4Url { get; set; }
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public Brand Brand { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
