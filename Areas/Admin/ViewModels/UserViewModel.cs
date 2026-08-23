namespace Presentation.Areas.Admin.ViewModels;

public class UserListViewModel
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string UserName { get; set; } = "";
    public string FullName { get; set; } = "";
    public bool EmailConfirmed { get; set; }
    public string Roles { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class EditRolesViewModel
{
    public string UserId { get; set; } = "";
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
    public List<string> CurrentRoles { get; set; } = new();
    public List<string> AllRoles { get; set; } = new();
    public List<string>? SelectedRoles { get; set; }
}
