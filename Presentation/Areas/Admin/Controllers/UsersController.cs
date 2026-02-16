using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Presentation.Areas.Admin.ViewModels;
using Shared.Constants;

namespace Presentation.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminAccess")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var query = _userManager.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u =>
                (u.Email != null && u.Email.ToLower().Contains(s)) ||
                (u.UserName != null && u.UserName.ToLower().Contains(s)) ||
                (u.FullName != null && u.FullName.ToLower().Contains(s)));
        }
        var total = await query.CountAsync(ct);
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var model = new List<UserListViewModel>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            model.Add(new UserListViewModel
            {
                Id = u.Id,
                Email = u.Email ?? "",
                UserName = u.UserName ?? "",
                FullName = u.FullName ?? "-",
                EmailConfirmed = u.EmailConfirmed,
                Roles = string.Join(", ", roles),
                CreatedAt = u.CreatedAt
            });
        }

        ViewBag.Total = total;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditRoles(string id, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        var userRoles = await _userManager.GetRolesAsync(user);
        var allRoles = await _roleManager.Roles.Where(r => r.Name != null).Select(r => r.Name!).ToListAsync(ct);
        return View(new EditRolesViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? user.UserName ?? "",
            FullName = user.FullName ?? "-",
            CurrentRoles = userRoles.ToList(),
            AllRoles = allRoles
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRoles(EditRolesViewModel vm, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(vm.UserId);
        if (user == null) return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);
        var toRemove = currentRoles.Except(vm.SelectedRoles ?? new List<string>()).ToList();
        var toAdd = (vm.SelectedRoles ?? new List<string>()).Except(currentRoles).ToList();

        if (toRemove.Contains(RoleNames.SuperAdmin))
        {
            var admins = await _userManager.GetUsersInRoleAsync(RoleNames.SuperAdmin);
            if (admins.Count <= 1)
            {
                TempData["Error"] = "Cannot remove the last SuperAdmin.";
                return RedirectToAction(nameof(EditRoles), new { id = vm.UserId });
            }
        }

        if (toRemove.Any()) await _userManager.RemoveFromRolesAsync(user, toRemove);
        if (toAdd.Any()) await _userManager.AddToRolesAsync(user, toAdd);

        TempData["Success"] = "Roles updated.";
        return RedirectToAction(nameof(Index));
    }
}
