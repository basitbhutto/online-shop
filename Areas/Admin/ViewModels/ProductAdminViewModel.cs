using System.ComponentModel.DataAnnotations;

namespace Presentation.Areas.Admin.ViewModels;

public class ProductListAdminViewModel
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string SKU { get; set; } = "";
    public string? ImageUrl { get; set; }
    public string CategoryName { get; set; } = "";
    public decimal SalePrice { get; set; }
    public int Stock { get; set; }
    public string Status { get; set; } = "";
}

public class ProductEditViewModel
{
    public long Id { get; set; }
    [Required(ErrorMessage = "Product name is required")]
    [Display(Name = "Product Name")]
    public string? Name { get; set; }
    [Required(ErrorMessage = "SKU is required")]
    public string? SKU { get; set; }
    [Required(ErrorMessage = "Category is required")]
    [Range(1, long.MaxValue, ErrorMessage = "Please select a category")]
    [Display(Name = "Category")]
    public long CategoryId { get; set; }
    public long? LocationId { get; set; }
    [Display(Name = "Purchase Price")]
    public decimal PurchasePrice { get; set; }
    [Required(ErrorMessage = "Sale price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Sale price must be greater than 0")]
    [Display(Name = "Sale Price")]
    public decimal SalePrice { get; set; }
    [Display(Name = "Discount Price")]
    public decimal? DiscountPrice { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
    public int Stock { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = "Active";
    public List<string> ImageUrls { get; set; } = new();
    public List<SpecItem> Specifications { get; set; } = new();
    public List<CategoryOption> Categories { get; set; } = new();
}

public class SpecItem
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
}
