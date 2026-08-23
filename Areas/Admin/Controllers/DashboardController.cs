using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminAccess")]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IRepository<Product> _productRepo;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardService dashboardService,
        IRepository<Product> productRepo,
        UserManager<ApplicationUser> userManager,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _productRepo = productRepo;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await _dashboardService.GetStatsAsync(cancellationToken);
            var year = DateTime.UtcNow.Year;
            var monthlySales = await _dashboardService.GetMonthlySalesAsync(year, cancellationToken);
            var productCount = await _productRepo.Query().CountAsync(cancellationToken);
            var userCount = await _userManager.Users.CountAsync(cancellationToken);
            ViewBag.Stats = stats;
            ViewBag.MonthlySales = monthlySales;
            ViewBag.ProductCount = productCount;
            ViewBag.UserCount = userCount;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dashboard error");
            ViewBag.Stats = new DashboardStatsDto(0, 0, 0, 0, 0);
            ViewBag.MonthlySales = Enumerable.Range(1, 12).Select(m => new MonthlySalesDto(m, 0, 0)).ToList();
            ViewBag.ProductCount = 0;
            ViewBag.UserCount = 0;
            ViewBag.Error = ex.Message;
            return View();
        }
    }
}
