using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class OrderStatusHistory
{
    [Key]
    public long Id { get; set; }
    public long OrderId { get; set; }
    public int Status { get; set; } // OrderStatus enum as int
    public string? ChangedByUserId { get; set; }
    public DateTime ChangedDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public Order Order { get; set; } = null!;
}
