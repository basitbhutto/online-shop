using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.ViewModels.Cart;

namespace Presentation.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var items = await _cartService.GetCartAsync(userId, cancellationToken);

        var model = items.Select(i => new CartItemViewModel(
            i.Id,
            i.Product.Id,
            i.Product.Name,
            i.Product.SKU,
            i.Variant?.VariantCombination,
            i.Quantity,
            i.Variant?.PriceOverride ?? (i.Product.DiscountPrice ?? i.Product.SalePrice),
            i.Product.Images.OrderBy(img => img.SortOrder).FirstOrDefault()?.ImageUrl,
            i.Product.Category.Name
        )).ToList();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int? variantId, int quantity = 1, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        await _cartService.AddToCartAsync(userId, productId, variantId, quantity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        await _cartService.RemoveFromCartAsync(userId, id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(int id, int quantity, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        await _cartService.UpdateQuantityAsync(userId, id, quantity, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
