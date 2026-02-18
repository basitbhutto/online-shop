using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class ProductAttribute
{
    [Key]
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AttributeFieldType FieldType { get; set; }
    public bool IsRequired { get; set; }

    public ICollection<CategoryAttribute> CategoryAttributes { get; set; } = new List<CategoryAttribute>();
    public ICollection<AttributeOption> Options { get; set; } = new List<AttributeOption>();
    public ICollection<ProductAttributeValue> ProductAttributeValues { get; set; } = new List<ProductAttributeValue>();
}
