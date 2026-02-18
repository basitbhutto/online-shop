using Application.Interfaces;
using Application.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Areas.Admin.ViewModels;

namespace Presentation.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminAccess")]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly IImageStorageService _imageStorage;

    public CategoriesController(ICategoryService categoryService, IImageStorageService imageStorage)
    {
        _categoryService = categoryService;
        _imageStorage = imageStorage;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var categories = await _categoryService.GetAllAsync(ct);
        var model = categories.Select(c => new CategoryViewModel
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            ImageUrl = c.ImageUrl,
            ParentId = c.ParentId,
            ParentName = c.Parent?.Name,
            Status = c.Status.ToString(),
            CreatedDate = c.CreatedDate
        }).ToList();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var roots = await _categoryService.GetRootCategoriesAsync(ct);
        return View(new CategoryCreateViewModel
        {
            ParentOptions = roots.Select(c => new CategoryOption { Id = c.Id, Name = c.Name }).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryCreateViewModel vm, IFormFile? Image, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(vm.Name))
            ModelState.AddModelError(nameof(vm.Name), "Name is required.");
        if (ModelState.IsValid)
        {
            string? imageUrl = null;
            if (Image != null && Image.Length > 0)
            {
                await using var stream = Image.OpenReadStream();
                imageUrl = await _imageStorage.SaveCategoryImageAsync(stream, Image.FileName, ct);
            }
            await _categoryService.CreateAsync(vm.Name.Trim(), vm.ParentId == 0 ? null : vm.ParentId, imageUrl, ct);
            TempData["Success"] = "Category created successfully.";
            return RedirectToAction(nameof(Index));
        }
        var roots = await _categoryService.GetRootCategoriesAsync(ct);
        vm.ParentOptions = roots.Select(c => new CategoryOption { Id = c.Id, Name = c.Name }).ToList();
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id, CancellationToken ct)
    {
        var cat = await _categoryService.GetByIdAsync(id, ct);
        if (cat == null) return NotFound();
        var roots = await _categoryService.GetRootCategoriesAsync(ct);
        return View(new CategoryEditViewModel
        {
            Id = cat.Id,
            Name = cat.Name,
            ImageUrl = cat.ImageUrl,
            ParentId = cat.ParentId,
            Status = cat.Status.ToString(),
            ParentOptions = roots.Where(c => c.Id != id).Select(c => new CategoryOption { Id = c.Id, Name = c.Name }).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryEditViewModel vm, IFormFile? Image, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(vm.Name))
            ModelState.AddModelError(nameof(vm.Name), "Name is required.");
        if (ModelState.IsValid)
        {
            string? imageUrl = vm.ImageUrl;
            if (Image != null && Image.Length > 0)
            {
                await using var stream = Image.OpenReadStream();
                imageUrl = await _imageStorage.SaveCategoryImageAsync(stream, Image.FileName, ct);
            }
            var status = Enum.TryParse<EntityStatus>(vm.Status, out var s) ? s : EntityStatus.Active;
            await _categoryService.UpdateAsync(vm.Id, vm.Name.Trim(), imageUrl, status, ct);
            TempData["Success"] = "Category updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        var roots = await _categoryService.GetRootCategoriesAsync(ct);
        vm.ParentOptions = roots.Where(c => c.Id != vm.Id).Select(c => new CategoryOption { Id = c.Id, Name = c.Name }).ToList();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _categoryService.DeleteAsync(id, ct);
        TempData["Success"] = "Category deleted.";
        return RedirectToAction(nameof(Index));
    }
}
