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

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int take = 8, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Ok(Array.Empty<object>());
        var products = await _productService.SearchProductsAsync(q.Trim(), null, null, null, null, null, null, 0, take, ct);
        return Ok(products.Select(p => new { p.Id, p.Name, p.MainImageUrl, displayPrice = p.DiscountPrice ?? p.SalePrice }));
    }

    [HttpGet("products/bulk")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductsBulk([FromQuery] string ids, CancellationToken ct = default)
    {
        var idList = ids?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => long.TryParse(s.Trim(), out var n) ? n : 0L)
            .Where(n => n > 0)
            .Distinct()
            .Take(50)
            .ToList() ?? new List<long>();
        if (idList.Count == 0) return Ok(Array.Empty<object>());
        var products = await _productService.GetProductsByIdsAsync(idList, ct);
        return Ok(products.Select(p => new { p.Id, p.Name, p.SKU, p.CategoryName, displayPrice = p.DiscountPrice ?? p.SalePrice, p.SalePrice, p.MainImageUrl }));
    }
}
