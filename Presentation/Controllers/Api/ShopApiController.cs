using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers.Api;

[ApiController]
[Route("api/shop")]
public class ShopApiController : ControllerBase
{
    private readonly IProductService _productService;

    public ShopApiController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("products/bulk")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductsBulk([FromQuery] string ids, CancellationToken ct = default)
    {
        var idList = ids?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s.Trim(), out var n) ? n : 0)
            .Where(n => n > 0)
            .Distinct()
            .Take(50)
            .ToList() ?? new List<int>();
        if (idList.Count == 0) return Ok(Array.Empty<object>());
        var products = await _productService.GetProductsByIdsAsync(idList, ct);
        return Ok(products.Select(p => new { p.Id, p.Name, p.SKU, p.CategoryName, displayPrice = p.DiscountPrice ?? p.SalePrice, p.SalePrice, p.MainImageUrl }));
    }
}
