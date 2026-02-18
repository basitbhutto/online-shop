using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class CartItem
{
    [Key]

    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public long ProductId { get; set; }
    public long? VariantId { get; set; }
    public int Quantity { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ProductVariant? Variant { get; set; }
}
