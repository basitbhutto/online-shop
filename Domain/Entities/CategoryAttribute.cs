using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class CategoryAttribute
{
    [Key]

    public long CategoryId { get; set; }
    public long AttributeId { get; set; }

    public Category Category { get; set; } = null!;
    public ProductAttribute Attribute { get; set; } = null!;
}
