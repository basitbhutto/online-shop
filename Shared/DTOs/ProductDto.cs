namespace Shared.DTOs;

public record ProductListDto(
    int Id,
    string Name,
    string SKU,
    string CategoryName,
    decimal SalePrice,
    decimal? DiscountPrice,
    int Stock,
    string? MainImageUrl
);

public record ProductDetailDto(
    int Id,
    string Name,
    string SKU,
    string CategoryName,
    decimal PurchasePrice,
    decimal SalePrice,
    decimal? DiscountPrice,
    int Stock,
    string? Description,
    List<string> ImageUrls,
    List<ProductAttributeValueDto> Attributes,
    List<ProductSpecificationDto> Specifications,
    List<ProductVariantDto> Variants
);

public record ProductSpecificationDto(string Key, string Value);

public record ProductAttributeValueDto(string AttributeName, string Value);

public record ProductVariantDto(
    int Id,
    string VariantCombination,
    int Stock,
    decimal? PriceOverride,
    string? SKU
);
