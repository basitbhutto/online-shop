namespace Presentation.Areas.Admin.ViewModels;

public class CategoryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedDate { get; set; }
}

public class CategoryEditViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int? ParentId { get; set; }
    public string Status { get; set; } = "Active";
    public List<CategoryOption> ParentOptions { get; set; } = new();
}

public class CategoryCreateViewModel
{
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public List<CategoryOption> ParentOptions { get; set; } = new();
}

public class CategoryOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
