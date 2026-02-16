using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Presentation.ViewModels.Checkout;
using Shared.Constants;

namespace Presentation.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;
    private readonly IEmailService _emailService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CheckoutController(ICartService cartService, IOrderService orderService, IEmailService emailService, UserManager<ApplicationUser> userManager)
    {
        _cartService = cartService;
        _orderService = orderService;
        _emailService = emailService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var cartItems = await _cartService.GetCartAsync(userId, cancellationToken);
        if (cartItems.Count == 0) return RedirectToAction("Index", "Cart");

        var model = new CheckoutViewModel
        {
            CartItems = cartItems.Select(i => new CheckoutItemViewModel(
                i.Id,
                i.Product.Id,
                i.Product.Name,
                i.VariantId,
                i.Variant?.VariantCombination,
                i.Quantity,
                i.Variant?.PriceOverride ?? (i.Product.DiscountPrice ?? i.Product.SalePrice)
            )).ToList(),
            TotalAmount = cartItems.Sum(i => i.Quantity * (i.Variant?.PriceOverride ?? (i.Product.DiscountPrice ?? i.Product.SalePrice))),
            City = AppConstants.KarachiCity
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutViewModel model, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var cartItems = await _cartService.GetCartAsync(userId, cancellationToken);
        if (cartItems.Count == 0) return RedirectToAction("Index", "Cart");

        if (string.IsNullOrWhiteSpace(model.ShippingAddress))
            ModelState.AddModelError(nameof(model.ShippingAddress), "Shipping address is required.");
        if (string.IsNullOrWhiteSpace(model.PhoneNumber))
            ModelState.AddModelError(nameof(model.PhoneNumber), "Phone number is required.");

        if (!string.Equals(model.City, AppConstants.KarachiCity, StringComparison.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(model.City), $"We currently deliver only to {AppConstants.KarachiCity}. Please enter Karachi as the city.");

        if (!ModelState.IsValid)
        {
            model.CartItems = cartItems.Select(i => new CheckoutItemViewModel(
                i.Id,
                i.Product.Id,
                i.Product.Name,
                i.VariantId,
                i.Variant?.VariantCombination,
                i.Quantity,
                i.Variant?.PriceOverride ?? (i.Product.DiscountPrice ?? i.Product.SalePrice)
            )).ToList();
            model.TotalAmount = cartItems.Sum(i => i.Quantity * (i.Variant?.PriceOverride ?? (i.Product.DiscountPrice ?? i.Product.SalePrice)));
            model.City = model.City ?? AppConstants.KarachiCity;
            return View("Index", model);
        }

        var items = cartItems.Select(i => (i.ProductId, i.VariantId, i.Quantity, i.Variant?.PriceOverride ?? (i.Product.DiscountPrice ?? i.Product.SalePrice))).ToList();
        var order = await _orderService.CreateOrderAsync(
            userId,
            model.ShippingAddress!,
            model.PhoneNumber ?? string.Empty,
            model.PreferredDeliveryTime,
            items,
            cancellationToken);

        if (order == null) return RedirectToAction("Index");

        await _cartService.ClearCartAsync(userId, cancellationToken);

        var user = await _userManager.FindByIdAsync(userId);
        if (user?.Email != null)
        {
            try
            {
                await _emailService.SendOrderConfirmationAsync(user.Email, user.UserName ?? user.Email, order.Id, order.TotalAmount, cancellationToken);
            }
            catch { /* Log but don't fail checkout */ }
        }

        return RedirectToAction("Confirmation", new { orderId = order.Id });
    }

    public async Task<IActionResult> Confirmation(int orderId, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var order = await _orderService.GetOrderByIdAsync(orderId, userId, cancellationToken);
        if (order == null) return NotFound();
        return View(order);
    }
}
