using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    [HttpPost("{productId:int}")]
    public async Task<IActionResult> Add(int productId, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        await _wishlistService.AddToWishlistAsync(userId, productId, cancellationToken);
        return Ok(new { success = true });
    }

    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> Remove(int productId, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        await _wishlistService.RemoveFromWishlistAsync(userId, productId, cancellationToken);
        return Ok(new { success = true });
    }

    [HttpGet("count")]
    public async Task<IActionResult> Count(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var items = await _wishlistService.GetWishlistAsync(userId, cancellationToken);
        return Ok(new { count = items.Count });
    }
}
