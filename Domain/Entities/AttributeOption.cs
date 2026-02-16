namespace Domain.Entities;

public class AttributeOption
{
    public int Id { get; set; }
    public int AttributeId { get; set; }
    public string Value { get; set; } = string.Empty;

    public ProductAttribute Attribute { get; set; } = null!;
}
