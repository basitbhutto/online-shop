using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class AttributeOption
{
    [Key]

    public long Id { get; set; }
    public long AttributeId { get; set; }
    public string Value { get; set; } = string.Empty;

    public ProductAttribute Attribute { get; set; } = null!;
}
