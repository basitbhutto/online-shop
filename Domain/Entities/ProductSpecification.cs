namespace Domain.Entities;

public class ProductSpecification
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string SpecKey { get; set; } = string.Empty;   // e.g. "Color", "Size"
    public string SpecValue { get; set; } = string.Empty; // e.g. "Red", "M"

    public Product Product { get; set; } = null!;
}
