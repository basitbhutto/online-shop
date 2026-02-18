using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Wishlist
{
    [Key]
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public long ProductId { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
