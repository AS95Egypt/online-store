using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStore.Application.Catalogs;
using OnlineStore.Application.Categories;

namespace OnlineStore.Api.Controllers;

[ApiController]
[Route("api/admin/categories")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminCategoriesController : ControllerBase
{
    private readonly IAdminCategoryService _categoryService;

    public AdminCategoriesController(IAdminCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllAsync(cancellationToken);
        return Ok(new { success = true, data = categories });
    }

    [HttpGet("{id:int}", Name = "GetAdminCategoryById")]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var category = await _categoryService.GetByIdAsync(id, cancellationToken);
        return category is null ? NotFound() : Ok(new { success = true, data = category });
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var category = await _categoryService.CreateAsync(request, cancellationToken);
            return CreatedAtRoute(
                "GetAdminCategoryById",
                new { id = category.Id },
                new { success = true, data = category });
        }
        catch (CatalogValidationException exception)
        {
            return ValidationError(exception);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync(
        int id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var category = await _categoryService.UpdateAsync(id, request, cancellationToken);
            return category is null
                ? NotFound()
                : Ok(new { success = true, data = category });
        }
        catch (CatalogValidationException exception)
        {
            return ValidationError(exception);
        }
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> SetStatusAsync(
        int id,
        [FromBody] UpdateCategoryStatusRequest request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryService.SetStatusAsync(id, request.IsActive, cancellationToken);
        return category is null ? NotFound() : Ok(new { success = true, data = category });
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
