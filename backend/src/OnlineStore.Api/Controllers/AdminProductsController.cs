using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStore.Application.Catalogs;
using OnlineStore.Application.Products;

namespace OnlineStore.Api.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminProductsController : ControllerBase
{
    private readonly IAdminProductService _productService;

    public AdminProductsController(IAdminProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPagedAsync(
        [FromQuery] string? page,
        [FromQuery] string? pageSize,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
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
        var result = await _productService.GetPagedAsync(
            new AdminProductListQuery(parsedPage, parsedPageSize, search),
            cancellationToken);
        return Ok(new { success = true, data = result.Data, meta = result.Meta });
    }

    [HttpGet("{id:int}", Name = "GetAdminProductById")]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        return product is null
            ? NotFound()
            : Ok(new { success = true, data = product });
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productService.CreateAsync(request, cancellationToken);
            return CreatedAtRoute(
                "GetAdminProductById",
                new { id = product.Id },
                new { success = true, data = product });
        }
        catch (CatalogValidationException exception)
        {
            return ValidationError(exception);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync(
        int id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productService.UpdateAsync(id, request, cancellationToken);
            return product is null
                ? NotFound()
                : Ok(new { success = true, data = product });
        }
        catch (CatalogValidationException exception)
        {
            return ValidationError(exception);
        }
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> SetStatusAsync(
        int id,
        [FromBody] UpdateProductStatusRequest request,
        CancellationToken cancellationToken)
    {
        var product = await _productService.SetStatusAsync(id, request.IsActive, cancellationToken);
        return product is null
            ? NotFound()
            : Ok(new { success = true, data = product });
    }

    private BadRequestObjectResult ValidationError(CatalogValidationException exception) =>
        BadRequest(new
        {
            success = false,
            error = new
            {
                code = exception.Code,
                message = exception.Message,
                details = exception.Errors,
            },
        });
}
