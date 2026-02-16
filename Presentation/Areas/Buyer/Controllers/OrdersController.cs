using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Presentation.Areas.Buyer.ViewModels;
using Shared.Constants;

namespace Presentation.Areas.Buyer.Controllers;

[Area("Buyer")]
[Authorize(Policy = "BuyerAccess")]
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(IOrderService orderService, UserManager<ApplicationUser> userManager)
    {
        _orderService = orderService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return RedirectToAction("Login", "Account", new { area = "" });

        var orders = await _orderService.GetUserOrdersAsync(userId, ct);
        var model = orders.Select(o => new OrderSummaryDto
        {
            Id = o.Id,
            Status = o.Status,
            TotalAmount = o.TotalAmount,
            CreatedDate = o.CreatedDate
        }).ToList();
        return View(model);
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct = default)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return RedirectToAction("Login", "Account", new { area = "" });

        var order = await _orderService.GetOrderByIdAsync(id, userId, ct);
        if (order == null) return NotFound();

        return View(order);
    }
}
