using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddToCartRequest request, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        await _cartService.AddToCartAsync(userId, request.ProductId, request.VariantId, request.Quantity, cancellationToken);
        var count = await _cartService.GetCartCountAsync(userId, cancellationToken);
        return Ok(new { success = true, cartCount = count });
    }

    [HttpGet("count")]
    public async Task<IActionResult> Count(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var count = await _cartService.GetCartCountAsync(userId, cancellationToken);
        return Ok(new { count });
    }

    [HttpGet("items")]
    public async Task<IActionResult> Items(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var items = await _cartService.GetCartAsync(userId, cancellationToken);
        var list = items.Select(i => new
        {
            id = i.Id,
            productId = i.ProductId,
            productName = i.Product.Name,
            imageUrl = i.Product.Images.OrderBy(img => img.SortOrder).FirstOrDefault()?.ImageUrl,
            quantity = i.Quantity,
            price = i.Variant?.PriceOverride ?? (i.Product.DiscountPrice ?? i.Product.SalePrice),
            variantCombination = i.Variant?.VariantCombination
        }).ToList();
        return Ok(list);
    }
}

public record AddToCartRequest(long ProductId, long? VariantId = null, int Quantity = 1);
