namespace Presentation.ViewModels.Cart;

public record CartItemViewModel(
    int Id,
    int ProductId,
    string ProductName,
    string SKU,
    string? VariantCombination,
    int Quantity,
    decimal Price,
    string? ImageUrl,
    string CategoryName
);
