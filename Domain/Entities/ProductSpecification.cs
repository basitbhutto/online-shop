using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class ProductSpecification
{
    [Key]
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string SpecKey { get; set; } = string.Empty;   // e.g. "Color", "Size"
    public string SpecValue { get; set; } = string.Empty; // e.g. "Red", "M"

    public Product Product { get; set; } = null!;
}
