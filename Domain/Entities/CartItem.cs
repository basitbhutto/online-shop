namespace Domain.Entities;

public class CartItem
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public int Quantity { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ProductVariant? Variant { get; set; }
}
