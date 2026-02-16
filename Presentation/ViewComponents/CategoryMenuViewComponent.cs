using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Presentation.ViewModels.Shop;

namespace Presentation.ViewComponents;

public class CategoryMenuViewComponent : ViewComponent
{
    private readonly ICategoryService _categoryService;

    public CategoryMenuViewComponent(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryService.GetRootCategoriesAsync(cancellationToken);
        var model = categories.Select(c => new CategoryViewModel(c.Id, c.Name, c.Slug)).ToList();
        return View(model);
    }
}
