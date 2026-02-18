namespace Presentation.ViewModels.Cart;

public record CartItemViewModel(
    long Id,
    long ProductId,
    string ProductName,
    string SKU,
    string? VariantCombination,
    int Quantity,
    decimal Price,
    string? ImageUrl,
    string CategoryName
);
