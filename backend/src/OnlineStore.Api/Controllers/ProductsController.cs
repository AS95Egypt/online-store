using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using OnlineStore.Domain.Entities;
using OnlineStore.Application.Products;

namespace OnlineStore.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? page,
        [FromQuery] string? pageSize,
        [FromQuery] string? search,
        [FromQuery] string? categoryId,
        [FromQuery] string? brandIds,
        [FromQuery] string? minPrice,
        [FromQuery] string? maxPrice,
        [FromQuery] string? stockStatus,
        [FromQuery] string? sort,
        CancellationToken cancellationToken)
    {
        var parsedPage = int.TryParse(page, NumberStyles.Integer, CultureInfo.InvariantCulture, out var requestedPage)
            ? requestedPage
            : ProductPaginationDefaults.Page;
        var parsedPageSize = int.TryParse(
            pageSize,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var requestedPageSize)
                ? requestedPageSize
                : ProductPaginationDefaults.PageSize;

        int? parsedCategoryId = null;
        if (!string.IsNullOrWhiteSpace(categoryId))
        {
            if (!int.TryParse(categoryId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var requestedCategoryId) ||
                requestedCategoryId <= 0)
            {
                return InvalidQuery("invalid_category_id", "categoryId must be a positive integer.");
            }

            parsedCategoryId = requestedCategoryId;
        }

        var parsedBrandIds = new List<int>();
        if (!string.IsNullOrWhiteSpace(brandIds))
        {
            foreach (var value in brandIds.Split(','))
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var brandId))
                {
                    return InvalidQuery("invalid_brand_ids", "brandIds must be a comma-separated list of integers.");
                }

                parsedBrandIds.Add(brandId);
            }
        }

        decimal? parsedMinPrice = null;
        decimal? parsedMaxPrice = null;
        if (!string.IsNullOrWhiteSpace(minPrice))
        {
            if (!decimal.TryParse(minPrice, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            {
                return InvalidQuery(
                    "invalid_price_range",
                    "minPrice must be less than or equal to maxPrice, and both must be non-negative.");
            }

            parsedMinPrice = value;
        }

        if (!string.IsNullOrWhiteSpace(maxPrice))
        {
            if (!decimal.TryParse(maxPrice, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            {
                return InvalidQuery(
                    "invalid_price_range",
                    "minPrice must be less than or equal to maxPrice, and both must be non-negative.");
            }

            parsedMaxPrice = value;
        }

        if (parsedMinPrice < 0 || parsedMaxPrice < 0 ||
            (parsedMinPrice is { } min && parsedMaxPrice is { } max && min > max))
        {
            return InvalidQuery(
                "invalid_price_range",
                "minPrice must be less than or equal to maxPrice, and both must be non-negative.");
        }

        var normalizedStockStatus = string.IsNullOrWhiteSpace(stockStatus) ? null : stockStatus.Trim();
        if (normalizedStockStatus is not null &&
            normalizedStockStatus != ProductStockStatuses.InStock &&
            normalizedStockStatus != ProductStockStatuses.OutOfStock)
        {
            return InvalidQuery("invalid_stock_status", "stockStatus must be InStock or OutOfStock.");
        }

        var normalizedSort = string.IsNullOrWhiteSpace(sort) ? null : sort.Trim();
        var parsedSort = normalizedSort switch
        {
            null => ProductSortOrder.None,
            "priceAsc" => ProductSortOrder.PriceAsc,
            "priceDesc" => ProductSortOrder.PriceDesc,
            _ => (ProductSortOrder?)null,
        };
        if (parsedSort is null)
        {
            return InvalidQuery("invalid_sort", "sort must be priceAsc or priceDesc.");
        }

        var result = await _productService.GetActiveProductsAsync(
            new ProductListQuery(
                parsedPage,
                parsedPageSize,
                search,
                parsedCategoryId,
                parsedBrandIds,
                parsedMinPrice,
                parsedMaxPrice,
                normalizedStockStatus,
                parsedSort.Value),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProductById(int id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetActiveProductByIdAsync(id, cancellationToken);

        if (product is null)
        {
            return NotFound(new
            {
                success = false,
                error = new
                {
                    code = "product_not_found",
                    message = "Product not found.",
                },
            });
        }

        return Ok(product);
    }

    private BadRequestObjectResult InvalidQuery(string code, string message)
    {
        return BadRequest(new
        {
            success = false,
            error = new
            {
                code,
                message,
            },
        });
    }
}
