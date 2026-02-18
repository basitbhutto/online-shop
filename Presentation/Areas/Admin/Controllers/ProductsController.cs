using Application.Interfaces;
using Application.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Areas.Admin.ViewModels;

namespace Presentation.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminAccess")]
public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IImageStorageService _imageStorage;
    private readonly ILocationService _locationService;

    public ProductsController(IProductService productService, ICategoryService categoryService, IImageStorageService imageStorage, ILocationService locationService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _imageStorage = imageStorage;
        _locationService = locationService;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var products = await _productService.GetAllForAdminAsync(ct);
        var model = products.Select(p => new ProductListAdminViewModel
        {
            Id = p.Id,
            Name = p.Name,
            SKU = p.SKU,
            ImageUrl = p.Images.OrderBy(i => i.SortOrder).FirstOrDefault()?.ImageUrl,
            CategoryName = p.Category?.Name ?? "-",
            SalePrice = p.SalePrice,
            Stock = p.Stock,
            Status = p.Status.ToString()
        }).ToList();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var categories = await _categoryService.GetAllAsync(ct);
        return View(new ProductEditViewModel { Categories = categories.Select(c => new CategoryOption { Id = c.Id, Name = c.Name }).ToList() });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductEditViewModel vm, IFormFile? CoverImage, IFormFileCollection? GalleryImages, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(vm.Name) || string.IsNullOrWhiteSpace(vm.SKU) || vm.CategoryId <= 0)
            ModelState.AddModelError(string.Empty, "Name, SKU and Category are required.");
        if (ModelState.IsValid)
        {
            var imageUrls = new List<string>();
            if (CoverImage != null && CoverImage.Length > 0)
            {
                await using var s = CoverImage.OpenReadStream();
                var url = await _imageStorage.SaveProductImageAsync(s, CoverImage.FileName, ct);
                if (url != null) imageUrls.Add(url);
            }
            if (GalleryImages != null)
                foreach (var f in GalleryImages.Where(x => x.Length > 0))
                {
                    await using var s = f.OpenReadStream();
                    var url = await _imageStorage.SaveProductImageAsync(s, f.FileName, ct);
                    if (url != null) imageUrls.Add(url);
                }
            var specs = (vm.Specifications ?? new List<SpecItem>()).Where(x => !string.IsNullOrWhiteSpace(x.Key)).Select(x => (x.Key, x.Value)).ToList();
            await _productService.CreateAsync(vm.Name!, vm.SKU!, vm.CategoryId, vm.PurchasePrice, vm.SalePrice, vm.DiscountPrice, vm.Stock, vm.Description, vm.LocationId, imageUrls, specs, ct);
            TempData["Success"] = "Product created.";
            return RedirectToAction(nameof(Index));
        }
        vm.Categories = (await _categoryService.GetAllAsync(ct)).Select(c => new CategoryOption { Id = c.Id, Name = c.Name }).ToList();
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id, CancellationToken ct)
    {
        var p = await _productService.GetByIdForAdminAsync(id, ct);
        if (p == null) return NotFound();
        var categories = await _categoryService.GetAllAsync(ct);
        return View(new ProductEditViewModel
        {
            Id = p.Id,
            Name = p.Name,
            SKU = p.SKU,
            CategoryId = p.CategoryId,
            PurchasePrice = p.PurchasePrice,
            SalePrice = p.SalePrice,
            DiscountPrice = p.DiscountPrice,
            Stock = p.Stock,
            Description = p.Description,
            Status = p.Status.ToString(),
            ImageUrls = p.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList(),
            Specifications = p.Specifications.Select(s => new SpecItem { Key = s.SpecKey, Value = s.SpecValue }).ToList(),
            Categories = categories.Select(c => new CategoryOption { Id = c.Id, Name = c.Name }).ToList(),
            LocationId = p.LocationId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductEditViewModel vm, IFormFile? CoverImage, IFormFileCollection? GalleryImages, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(vm.Name) || string.IsNullOrWhiteSpace(vm.SKU) || vm.CategoryId <= 0)
            ModelState.AddModelError(string.Empty, "Name, SKU and Category are required.");
        if (ModelState.IsValid && vm.Id > 0)
        {
            var imageUrls = new List<string>(vm.ImageUrls ?? new List<string>());
            if (CoverImage != null && CoverImage.Length > 0)
            {
                await using var s = CoverImage.OpenReadStream();
                var url = await _imageStorage.SaveProductImageAsync(s, CoverImage.FileName, ct);
                if (url != null) imageUrls.Insert(0, url);
            }
            if (GalleryImages != null)
                foreach (var f in GalleryImages.Where(x => x.Length > 0))
                {
                    await using var s = f.OpenReadStream();
                    var url = await _imageStorage.SaveProductImageAsync(s, f.FileName, ct);
                    if (url != null) imageUrls.Add(url);
                }
            var specs = (vm.Specifications ?? new List<SpecItem>()).Where(x => !string.IsNullOrWhiteSpace(x.Key)).Select(x => (x.Key, x.Value)).ToList();
            var status = Enum.TryParse<EntityStatus>(vm.Status, out var st) ? st : EntityStatus.Active;
            await _productService.UpdateAsync(vm.Id, vm.Name!, vm.SKU ?? "", vm.CategoryId, vm.PurchasePrice, vm.SalePrice, vm.DiscountPrice, vm.Stock, vm.Description, status, vm.LocationId, imageUrls, specs, ct);
            TempData["Success"] = "Product updated.";
            return RedirectToAction(nameof(Index));
        }
        vm.Categories = (await _categoryService.GetAllAsync(ct)).Select(c => new CategoryOption { Id = c.Id, Name = c.Name }).ToList();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _productService.DeleteAsync(id, ct);
        TempData["Success"] = "Product deleted.";
        return RedirectToAction(nameof(Index));
    }
}
