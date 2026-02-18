using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class OrderItem
{
    [Key]
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public long? VariantId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ProductVariant? Variant { get; set; }
}
