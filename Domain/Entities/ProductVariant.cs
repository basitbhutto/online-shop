using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class ProductVariant
{
    [Key]
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string VariantCombination { get; set; } = string.Empty; // JSON format: {"Size":"L","Color":"Red"}
    public int Stock { get; set; }
    public decimal? PriceOverride { get; set; }
    public string? SKU { get; set; }

    public Product Product { get; set; } = null!;
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
