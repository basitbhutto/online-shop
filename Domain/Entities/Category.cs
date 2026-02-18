using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Category
{
    [Key]

    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public EntityStatus Status { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<CategoryAttribute> CategoryAttributes { get; set; } = new List<CategoryAttribute>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
