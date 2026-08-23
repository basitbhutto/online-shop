using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Presentation.Areas.Buyer.ViewModels;
using Shared.Constants;

namespace Presentation.Areas.Buyer.Controllers;

[Area("Buyer")]
[Authorize(Policy = "BuyerAccess")]
public class DashboardController : Controller
{
    private readonly IOrderService _orderService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(IOrderService orderService, UserManager<ApplicationUser> userManager)
    {
        _orderService = orderService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return RedirectToAction("Login", "Account", new { area = "" });

        var user = await _userManager.FindByIdAsync(userId);
        var orders = await _orderService.GetUserOrdersAsync(userId, ct);
        var recent = orders.Take(5).Select(o => new OrderSummaryDto
        {
            Id = o.Id,
            Status = o.Status,
            TotalAmount = o.TotalAmount,
            CreatedDate = o.CreatedDate
        }).ToList();

        return View(new BuyerDashboardViewModel
        {
            FullName = user?.FullName ?? user?.UserName ?? "User",
            Email = user?.Email ?? "",
            TotalOrders = orders.Count,
            RecentOrders = recent
        });
    }
}
