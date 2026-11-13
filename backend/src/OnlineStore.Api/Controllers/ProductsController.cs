using System.Globalization;
using Microsoft.AspNetCore.Mvc;
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

        var result = await _productService.GetActiveProductsAsync(
            parsedPage,
            parsedPageSize,
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
}
