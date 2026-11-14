using Microsoft.AspNetCore.Mvc;
using OnlineStore.Application.Brands;

namespace OnlineStore.Api.Controllers;

[ApiController]
[Route("api/brands")]
public sealed class BrandsController : ControllerBase
{
    private readonly IBrandService _brandService;

    public BrandsController(IBrandService brandService)
    {
        _brandService = brandService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBrands(CancellationToken cancellationToken)
    {
        var brands = await _brandService.GetBrandsAsync(cancellationToken);
        return Ok(new { success = true, data = brands });
    }
}
