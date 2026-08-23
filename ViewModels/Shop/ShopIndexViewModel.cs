namespace Presentation.ViewModels.Shop;

public record ShopIndexViewModel
{
    public List<ProductCardViewModel> Products { get; init; } = new();
    public List<CategoryViewModel> Categories { get; init; } = new();
    public List<LocationFilterViewModel> LocationOptions { get; init; } = new();
    public string? SearchTerm { get; init; }
    public long? SelectedCategoryId { get; init; }
    public long? SelectedLocationId { get; init; }
    public decimal? UserLat { get; init; }
    public decimal? UserLng { get; init; }
    public string? CategoryName { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public int TotalCount { get; init; }
    public int CurrentPage { get; init; } = 1;
    public int TotalPages { get; init; } = 1;
}

public record LocationFilterViewModel(long Id, string DisplayName);

public record ProductCardViewModel(
    long Id,
    string Name,
    string SKU,
    string CategoryName,
    decimal DisplayPrice,
    decimal SalePrice,
    int Stock,
    string? MainImageUrl,
    bool IsInWishlist,
    string? LocationName
);

public record CategoryViewModel(long Id, string Name, string Slug);
