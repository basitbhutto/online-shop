using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var orders = await _orderService.GetUserOrdersAsync(userId, cancellationToken);
        return View(orders);
    }

    public async Task<IActionResult> Details(long id, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var order = await _orderService.GetOrderByIdAsync(id, userId, cancellationToken);
        if (order == null) return NotFound();
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(long id, string? notes, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var ok = await _orderService.CancelOrderAsync(id, userId, isAdmin: false, notes, cancellationToken);
        if (!ok) TempData["Error"] = "This order cannot be cancelled (e.g. already shipped).";
        return RedirectToAction(nameof(Details), new { id });
    }
}
