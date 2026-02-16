namespace Domain.Entities;

public class OrderStatusHistory
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int Status { get; set; } // OrderStatus enum as int
    public string? ChangedByUserId { get; set; }
    public DateTime ChangedDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public Order Order { get; set; } = null!;
}
