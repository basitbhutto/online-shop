namespace Domain.Entities;

public class ProductAttributeValue
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int AttributeId { get; set; }
    public string Value { get; set; } = string.Empty;

    public Product Product { get; set; } = null!;
    public ProductAttribute Attribute { get; set; } = null!;
}
