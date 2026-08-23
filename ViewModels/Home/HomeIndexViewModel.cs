namespace Presentation.ViewModels.Home;

public class HomeIndexViewModel
{
    public List<CategoryItem> Categories { get; set; } = new();
    public List<ProductCardItem> Products { get; set; } = new();
}

public class CategoryItem
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
}

public class ProductCardItem
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string SKU { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public decimal DisplayPrice { get; set; }
    public decimal SalePrice { get; set; }
    public int Stock { get; set; }
    public string? MainImageUrl { get; set; }
    public bool IsInWishlist { get; set; }
    public string? LocationName { get; set; }
}
