using System.ComponentModel.DataAnnotations;

namespace Presentation.ViewModels.Checkout;

public class CheckoutViewModel
{
    public List<CheckoutItemViewModel> CartItems { get; set; } = new();
    public decimal TotalAmount { get; set; }

    [Required]
    [Display(Name = "Shipping Address")]
    [StringLength(500)]
    public string? ShippingAddress { get; set; }

    [Required]
    [Display(Name = "City")]
    [StringLength(100)]
    public string City { get; set; } = "Karachi";

    [Required]
    [Display(Name = "Phone Number")]
    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Preferred Delivery Time")]
    [StringLength(100)]
    public string? PreferredDeliveryTime { get; set; }
}

public record CheckoutItemViewModel(long CartItemId, long ProductId, string ProductName, long? VariantId, string? VariantCombination, int Quantity, decimal Price);
