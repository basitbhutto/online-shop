using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    [HttpPost("{productId:int}")]
    [Authorize]
    public async Task<IActionResult> Add(int productId, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        await _wishlistService.AddToWishlistAsync(userId, productId, cancellationToken);
        return Ok(new { success = true });
    }

    [HttpDelete("{productId:int}")]
    [Authorize]
    public async Task<IActionResult> Remove(int productId, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        await _wishlistService.RemoveFromWishlistAsync(userId, productId, cancellationToken);
        return Ok(new { success = true });
    }

    [HttpGet("count")]
    [Authorize]
    public async Task<IActionResult> Count(CancellationToken cancellationToken = default)
    {
        var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Ok(new { count = 0 });
        var items = await _wishlistService.GetWishlistAsync(userId, cancellationToken);
        return Ok(new { count = items?.Count ?? 0 });
    }

    [HttpPost("sync")]
    [Authorize]
    public async Task<IActionResult> Sync([FromBody] SyncWishlistRequest? req, CancellationToken cancellationToken = default)
    {
        if (req?.ProductIds == null || req.ProductIds.Count == 0) return Ok(new { synced = 0 });
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var synced = 0;
        foreach (var pid in req.ProductIds.Distinct())
        {
            try
            {
                await _wishlistService.AddToWishlistAsync(userId, pid, cancellationToken);
                synced++;
            }
            catch { /* ignore duplicates */ }
        }
        return Ok(new { synced });
    }
}

public class SyncWishlistRequest { public List<int>? ProductIds { get; set; } }
