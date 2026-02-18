namespace Presentation.ViewModels.Shop;

public record ProductDetailViewModel(
    long Id,
    string Name,
    string SKU,
    string CategoryName,
    decimal SalePrice,
    decimal? DiscountPrice,
    int Stock,
    string? Description,
    List<string> ImageUrls,
    List<ProductAttributeDisplayViewModel> Attributes,
    List<ProductSpecificationViewModel> Specifications,
    List<ProductVariantViewModel> Variants,
    bool IsInWishlist
);

public record ProductSpecificationViewModel(string Key, string Value);

public record ProductAttributeDisplayViewModel(string Name, string Value);

public record ProductVariantViewModel(long Id, string Combination, int Stock, decimal Price, string? SKU);
