namespace Domain.Entities;

public class CategoryAttribute
{
    public int CategoryId { get; set; }
    public int AttributeId { get; set; }

    public Category Category { get; set; } = null!;
    public ProductAttribute Attribute { get; set; } = null!;
}
