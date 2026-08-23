using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.ViewModels.Shop;

namespace Presentation.Controllers;

public class ShopController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly IProductService _productService;
    private readonly IWishlistService _wishlistService;
    private readonly ICartService _cartService;
    private readonly ILocationService _locationService;

    public ShopController(ICategoryService categoryService, IProductService productService, IWishlistService wishlistService, ICartService cartService, ILocationService locationService)
    {
        _categoryService = categoryService;
        _productService = productService;
        _wishlistService = wishlistService;
        _cartService = cartService;
        _locationService = locationService;
    }

    public async Task<IActionResult> Index(string? search, long? categoryId, decimal? minPrice, decimal? maxPrice, long? locationId, decimal? lat, decimal? lng, long? firstId, int page = 1, int pageSize = 12, CancellationToken cancellationToken = default)
    {
        var skip = (page - 1) * pageSize;
        var products = await _productService.SearchProductsAsync(search, categoryId, minPrice, maxPrice, locationId, lat, lng, skip, pageSize, cancellationToken);
        var categories = await _categoryService.GetRootCategoriesAsync(cancellationToken);
        var locationOptions = (await _locationService.GetLocationsWithProductsAsync(cancellationToken))
            .Select(l => new LocationFilterViewModel(l.Id, l.FullPath)).ToList();
        var allProducts = await _productService.SearchProductsAsync(search, categoryId, minPrice, maxPrice, locationId, null, null, 0, int.MaxValue, cancellationToken);
        var totalCount = allProducts.Count;

        var productCards = new List<ProductCardViewModel>();
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        foreach (var p in products)
        {
            var isInWishlist = userId != null && await _wishlistService.IsInWishlistAsync(userId, p.Id, cancellationToken);
            productCards.Add(new ProductCardViewModel(p.Id, p.Name, p.SKU, p.CategoryName, p.DiscountPrice ?? p.SalePrice, p.SalePrice, p.Stock, p.MainImageUrl, isInWishlist, p.LocationName));
        }

        if (firstId.HasValue && firstId.Value > 0)
        {
            var firstProducts = await _productService.GetProductsByIdsAsync(new[] { firstId.Value }, cancellationToken);
            var firstProduct = firstProducts.FirstOrDefault();
            if (firstProduct != null)
            {
                var isInWishlist = userId != null && await _wishlistService.IsInWishlistAsync(userId, firstProduct.Id, cancellationToken);
                var firstCard = new ProductCardViewModel(firstProduct.Id, firstProduct.Name, firstProduct.SKU, firstProduct.CategoryName, firstProduct.DiscountPrice ?? firstProduct.SalePrice, firstProduct.SalePrice, firstProduct.Stock, firstProduct.MainImageUrl, isInWishlist, firstProduct.LocationName);
                productCards.RemoveAll(c => c.Id == firstProduct.Id);
                productCards.Insert(0, firstCard);
            }
        }

        var model = new ShopIndexViewModel
        {
            Products = productCards,
            Categories = categories.Select(c => new CategoryViewModel(c.Id, c.Name, c.Slug)).ToList(),
            LocationOptions = locationOptions,
            SearchTerm = search,
            SelectedCategoryId = categoryId,
            SelectedLocationId = locationId,
            UserLat = lat,
            UserLng = lng,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            TotalCount = totalCount,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return View(model);
    }

    [Route("Category/{slug}")]
    public async Task<IActionResult> Category(string slug, int page = 1, int pageSize = 12, CancellationToken cancellationToken = default)
    {
        var category = await _categoryService.GetBySlugAsync(slug, cancellationToken);
        if (category == null) return NotFound();

        var skip = (page - 1) * pageSize;
        var products = await _productService.GetProductsByCategoryAsync(category.Id, skip, pageSize, cancellationToken);
        var categories = await _categoryService.GetRootCategoriesAsync(cancellationToken);
        var totalCount = (await _productService.GetProductsByCategoryAsync(category.Id, 0, int.MaxValue, cancellationToken)).Count;

        var productCards = new List<ProductCardViewModel>();
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        foreach (var p in products)
        {
            var isInWishlist = userId != null && await _wishlistService.IsInWishlistAsync(userId, p.Id, cancellationToken);
            productCards.Add(new ProductCardViewModel(p.Id, p.Name, p.SKU, p.CategoryName, p.DiscountPrice ?? p.SalePrice, p.SalePrice, p.Stock, p.MainImageUrl, isInWishlist, p.LocationName));
        }

        var locationOptions = (await _locationService.GetLocationsWithProductsAsync(cancellationToken))
            .Select(l => new LocationFilterViewModel(l.Id, l.FullPath)).ToList();
        var model = new ShopIndexViewModel
        {
            Products = productCards,
            Categories = categories.Select(c => new CategoryViewModel(c.Id, c.Name, c.Slug)).ToList(),
            LocationOptions = locationOptions,
            SelectedCategoryId = category.Id,
            CategoryName = category.Name,
            TotalCount = totalCount,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return View("Index", model);
    }

    [Route("Product/Details/{id:int}")]
    public async Task<IActionResult> Details(long id, CancellationToken cancellationToken = default)
    {
        var product = await _productService.GetProductDetailAsync(id, cancellationToken);
        if (product == null) return NotFound();

        var isInWishlist = false;
        if (User.Identity?.IsAuthenticated == true)
            isInWishlist = await _wishlistService.IsInWishlistAsync(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value, id, cancellationToken);

        var specs = (product.Specifications ?? new List<Shared.DTOs.ProductSpecificationDto>()).Select(s => new ProductSpecificationViewModel(s.Key, s.Value)).ToList();
        var model = new ProductDetailViewModel(
            product.Id, product.Name, product.SKU, product.CategoryName,
            product.SalePrice, product.DiscountPrice, product.Stock, product.Description!,
            product.ImageUrls,
            product.Attributes.Select(a => new ProductAttributeDisplayViewModel(a.AttributeName, a.Value)).ToList(),
            specs,
            product.Variants.Select(v => new ProductVariantViewModel(v.Id, v.VariantCombination, v.Stock, v.PriceOverride ?? product.SalePrice, v.SKU)).ToList(),
            isInWishlist
        );

        return View(model);
    }

    [Route("Product/DetailsPartial/{id:int}")]
    public async Task<IActionResult> DetailsPartial(long id, CancellationToken cancellationToken = default)
    {
        var product = await _productService.GetProductDetailAsync(id, cancellationToken);
        if (product == null) return NotFound();

        var isInWishlist = false;
        if (User.Identity?.IsAuthenticated == true)
            isInWishlist = await _wishlistService.IsInWishlistAsync(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value, id, cancellationToken);

        var specs = (product.Specifications ?? new List<Shared.DTOs.ProductSpecificationDto>()).Select(s => new ProductSpecificationViewModel(s.Key, s.Value)).ToList();
        var model = new ProductDetailViewModel(
            product.Id, product.Name, product.SKU, product.CategoryName,
            product.SalePrice, product.DiscountPrice, product.Stock, product.Description!,
            product.ImageUrls,
            product.Attributes.Select(a => new ProductAttributeDisplayViewModel(a.AttributeName, a.Value)).ToList(),
            specs,
            product.Variants.Select(v => new ProductVariantViewModel(v.Id, v.VariantCombination, v.Stock, v.PriceOverride ?? product.SalePrice, v.SKU)).ToList(),
            isInWishlist
        );

        return PartialView("_ProductDetailModalContent", model);
    }

    [Route("Product/BuyNow/{id:int}")]
    [Authorize]
    public async Task<IActionResult> BuyNow(long id, CancellationToken cancellationToken = default)
    {
        var product = await _productService.GetProductDetailAsync(id, cancellationToken);
        if (product == null) return NotFound();

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        long? variantId = product.Variants?.FirstOrDefault()?.Id;
        await _cartService.AddToCartAsync(userId, id, variantId, 1, cancellationToken);
        return RedirectToAction("Index", "Checkout");
    }
}
