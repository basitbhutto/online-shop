namespace Domain.Entities;

public class ProductChatThread
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string BuyerUserId { get; set; } = string.Empty;

    public Product Product { get; set; } = null!;
    public ApplicationUser Buyer { get; set; } = null!;
    public ICollection<ProductChatMessage> Messages { get; set; } = new List<ProductChatMessage>();
}
