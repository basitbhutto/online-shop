using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.ViewModels.Shop;

namespace Presentation.Controllers;

[Authorize]
public class WishlistController : Controller
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return RedirectToAction("Login", "Account");

        var items = await _wishlistService.GetWishlistAsync(userId, ct);
        var model = items.Select(w => new ProductCardViewModel(
            w.Product!.Id,
            w.Product.Name,
            w.Product.SKU,
            w.Product.Category?.Name ?? "",
            w.Product.DiscountPrice ?? w.Product.SalePrice,
            w.Product.SalePrice,
            w.Product.Stock,
            w.Product.Images?.OrderBy(i => i.SortOrder).FirstOrDefault()?.ImageUrl,
            true
        )).ToList();

        return View(model);
    }
}
