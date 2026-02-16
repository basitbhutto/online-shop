using Domain.Enums;

namespace Domain.Entities;

public class ActivityLog
{
    public long Id { get; set; }
    public string? UserId { get; set; }
    public string? IPAddress { get; set; }
    public string? Device { get; set; }
    public string? Browser { get; set; }
    public ActivityActionType ActionType { get; set; }
    public string? PageUrl { get; set; }
    public int? ProductId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
    public Product? Product { get; set; }
}
