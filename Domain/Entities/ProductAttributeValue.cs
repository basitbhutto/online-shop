using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class ProductAttributeValue
{
    [Key]
    public long Id { get; set; }
    public long ProductId { get; set; }
    public long AttributeId { get; set; }
    public string Value { get; set; } = string.Empty;

    public Product Product { get; set; } = null!;
    public ProductAttribute Attribute { get; set; } = null!;
}
