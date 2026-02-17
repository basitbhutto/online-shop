namespace Domain.Entities;

public class ProductChatMessage
{
    public int Id { get; set; }
    public int ThreadId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsFromAdmin { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ProductChatThread Thread { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
