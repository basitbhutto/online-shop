using System.ComponentModel.DataAnnotations;

namespace Presentation.ViewModels.Account;

public class VerifyOtpViewModel
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    [StringLength(6, MinimumLength = 6)]
    [Display(Name = "Verification Code")]
    public string? OTP { get; set; }
}
