using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

/// <summary>
/// N-level hierarchical location (e.g. Country → Province → City → Area/Block) with optional coordinates.
/// </summary>
public class Location
{
    [Key]

    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    /// <summary>Denormalized path e.g. "Karachi, Sindh, Pakistan" for display.</summary>
    public string? FullPath { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int DisplayOrder { get; set; }

    public Location? Parent { get; set; }
    public ICollection<Location> Children { get; set; } = new List<Location>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
