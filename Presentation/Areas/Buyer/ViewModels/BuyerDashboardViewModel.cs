using Domain.Enums;

namespace Presentation.Areas.Buyer.ViewModels;

public class BuyerDashboardViewModel
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public int TotalOrders { get; set; }
    public List<OrderSummaryDto> RecentOrders { get; set; } = new();
}

public class OrderSummaryDto
{
    public int Id { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusText => Status.ToString().Replace("_", " ");
    public decimal TotalAmount { get; set; }
    public DateTime CreatedDate { get; set; }
}
