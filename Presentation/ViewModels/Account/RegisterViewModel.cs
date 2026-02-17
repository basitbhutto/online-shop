using System.ComponentModel.DataAnnotations;

namespace Presentation.ViewModels.Account;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Full name is required")]
    [Display(Name = "Full Name")]
    [StringLength(100)]
    public string? FullName { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string? Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password")]
    public string? ConfirmPassword { get; set; }
}
