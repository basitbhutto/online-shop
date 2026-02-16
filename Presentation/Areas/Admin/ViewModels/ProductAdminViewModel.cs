namespace Presentation.Areas.Admin.ViewModels;

public class ProductListAdminViewModel
{
    public int Id { get; set; }
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
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? SKU { get; set; }
    public int CategoryId { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal? DiscountPrice { get; set; }
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
