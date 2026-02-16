using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;
using Presentation.ViewModels.Home;
using System.Diagnostics;

namespace Presentation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IWishlistService _wishlistService;

        public HomeController(ICategoryService categoryService, IProductService productService, IWishlistService wishlistService)
        {
            _categoryService = categoryService;
            _productService = productService;
            _wishlistService = wishlistService;
        }

        public async Task<IActionResult> Index(CancellationToken ct = default)
        {
            var cats = await _categoryService.GetRootCategoriesAsync(ct);
            var products = await _productService.SearchProductsAsync(null, null, null, null, 0, 12, ct);
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var productCards = new List<ProductCardItem>();
            foreach (var p in products)
            {
                var isInWishlist = userId != null && await _wishlistService.IsInWishlistAsync(userId, p.Id, ct);
                productCards.Add(new ProductCardItem
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    CategoryName = p.CategoryName,
                    DisplayPrice = p.DiscountPrice ?? p.SalePrice,
                    SalePrice = p.SalePrice,
                    Stock = p.Stock,
                    MainImageUrl = p.MainImageUrl,
                    IsInWishlist = isInWishlist
                });
            }
            var model = new HomeIndexViewModel
            {
                Categories = cats.Select(c => new CategoryItem { Id = c.Id, Name = c.Name, Slug = c.Slug }).ToList(),
                Products = productCards
            };
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
