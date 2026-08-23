using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Presentation.ViewModels.Account;
using Application.Interfaces;

namespace Presentation.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailService _emailService;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        ViewData["ReturnUrl"] = returnUrl;
        var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        if (!ModelState.IsValid)
        {
            if (isAjax) return Json(new { success = false, message = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email!);
        if (user != null && !user.EmailVerified)
        {
            if (isAjax) return Json(new { success = false, message = "Please verify your email first. Check your inbox for the OTP." });
            ModelState.AddModelError(string.Empty, "Please verify your email first. Check your inbox for the OTP.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(model.Email!, model.Password!, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            var signedUser = await _userManager.FindByEmailAsync(model.Email!);
            var redirect = returnUrl ?? "/";
            if (signedUser != null && (await _userManager.IsInRoleAsync(signedUser, "SuperAdmin") || await _userManager.IsInRoleAsync(signedUser, "AdminStaff")))
                redirect = returnUrl ?? "/Admin";
            if (isAjax) return Json(new { success = true, redirect = Url.IsLocalUrl(redirect) ? redirect : "/" });
            return LocalRedirect(redirect);
        }

        if (isAjax) return Json(new { success = false, message = "Invalid login attempt." });
        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        ViewData["ReturnUrl"] = returnUrl;
        var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        if (!ModelState.IsValid)
        {
            if (isAjax) return Json(new { success = false, message = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });
            return View(model);
        }

        var existingUser = await _userManager.FindByEmailAsync(model.Email!);
        if (existingUser != null)
        {
            if (existingUser.EmailVerified)
            {
                if (isAjax) return Json(new { success = false, message = "An account with this email already exists. Please login." });
                ModelState.AddModelError(string.Empty, "An account with this email already exists. Please login.");
                return View(model);
            }
            await _userManager.DeleteAsync(existingUser);
        }

        var otp = new Random().Next(100000, 999999).ToString();
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            EmailVerified = false,
            OTPCode = otp,
            OTPExpiry = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password!);
        if (!result.Succeeded)
        {
            var errMsg = string.Join(" ", result.Errors.Select(e => e.Description));
            if (isAjax) return Json(new { success = false, message = errMsg });
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, "Buyer");

        try
        {
            await _emailService.SendOtpAsync(user.Email!, otp, cancellationToken);
        }
        catch (Exception)
        {
            if (isAjax) return Json(new { success = false, message = "Could not send verification email. Please try again later." });
            ModelState.AddModelError(string.Empty, "Could not send verification email. Please try again later.");
            return View(model);
        }

        TempData["VerifyEmail"] = model.Email;
        var verifyUrl = Url.Action(nameof(VerifyOtp), new { returnUrl });
        if (isAjax) return Json(new { success = true, redirect = verifyUrl });
        return Redirect(verifyUrl!);
    }

    [HttpGet]
    public IActionResult VerifyOtp(string? returnUrl = null)
    {
        var email = TempData["VerifyEmail"]?.ToString();
        if (string.IsNullOrEmpty(email))
            return RedirectToAction(nameof(Login));

        ViewData["ReturnUrl"] = returnUrl;
        return View(new VerifyOtpViewModel { Email = email });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email!);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid request.");
            return View(model);
        }

        if (user.OTPCode != model.OTP || user.OTPExpiry == null || user.OTPExpiry < DateTime.UtcNow)
        {
            ModelState.AddModelError(string.Empty, "Invalid or expired OTP. Please try again or resend code.");
            return View(model);
        }

        user.EmailVerified = true;
        user.OTPCode = null;
        user.OTPExpiry = null;
        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);

        await _signInManager.SignInAsync(user, isPersistent: false);
        return LocalRedirect(returnUrl ?? "/");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp(string email, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return Json(new { success = false });
        var otp = new Random().Next(100000, 999999).ToString();
        user.OTPCode = otp;
        user.OTPExpiry = DateTime.UtcNow.AddMinutes(5);
        await _userManager.UpdateAsync(user);
        await _emailService.SendOtpAsync(email, otp, cancellationToken);
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(string? returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        return LocalRedirect(returnUrl ?? "/");
    }
}
