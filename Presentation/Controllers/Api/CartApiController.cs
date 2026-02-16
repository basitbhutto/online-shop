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
}

public record AddToCartRequest(int ProductId, int? VariantId = null, int Quantity = 1);
