using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStore.Application.Brands;
using OnlineStore.Application.Catalogs;

namespace OnlineStore.Api.Controllers;

[ApiController]
[Route("api/admin/brands")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminBrandsController : ControllerBase
{
    private readonly IAdminBrandService _brandService;

    public AdminBrandsController(IAdminBrandService brandService)
    {
        _brandService = brandService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var brands = await _brandService.GetAllAsync(cancellationToken);
        return Ok(new { success = true, data = brands });
    }

    [HttpGet("{id:int}", Name = "GetAdminBrandById")]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var brand = await _brandService.GetByIdAsync(id, cancellationToken);
        return brand is null ? NotFound() : Ok(new { success = true, data = brand });
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateBrandRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var brand = await _brandService.CreateAsync(request, cancellationToken);
            return CreatedAtRoute(
                "GetAdminBrandById",
                new { id = brand.Id },
                new { success = true, data = brand });
        }
        catch (CatalogValidationException exception)
        {
            return ValidationError(exception);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync(
        int id,
        [FromBody] UpdateBrandRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var brand = await _brandService.UpdateAsync(id, request, cancellationToken);
            return brand is null
                ? NotFound()
                : Ok(new { success = true, data = brand });
        }
        catch (CatalogValidationException exception)
        {
            return ValidationError(exception);
        }
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
