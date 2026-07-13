using CoursesApp.Web.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;

namespace CoursesApp.Web.Controllers;

public class AuthController(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IEmailService emailService,
    IEmailDomainValidator domainValidator) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }
        
        var result = await signInManager.PasswordSignInAsync(
            dto.Email, dto.Password, dto.RememberMe, lockoutOnFailure: false);

        if (result.IsNotAllowed)
        {
            ModelState.AddModelError(string.Empty, "Please confirm your email before logging in.");
            return View(dto);
        }
        
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password");
            return View(dto);
        }
        
        return LocalRedirect(returnUrl ?? "/");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(bool rateLimited = false)
    {
        if (rateLimited)
        {
            ModelState.AddModelError(string.Empty, "Too many registration attempts. Please try again in a few minutes.");
        }
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimiterPolicyNames.Register)]
    public async Task<IActionResult> Register(RegisterDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        if (dto.Password != dto.ConfirmPassword)
        {
            ModelState.AddModelError(string.Empty, "Passwords do not match");
            return View(dto);
        }
        
        var existingUser = await userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
        {
            ModelState.AddModelError(nameof(dto.Email), "This email is already registered");
            return View(dto);
        }

        if (!await domainValidator.HasMxRecordAsync(dto.Email, ct))
        {
            ModelState.AddModelError(nameof(dto.Email), "Email domain does not exist or cannot receive mail");
            return View(dto);
        }

        var user = new AppUser
        {
            UserName = dto.Email,
            Email = dto.Email
        };

        var result = await userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            
            return View(dto);
        }
        
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmLink = Url.Action("ConfirmEmail", "Auth", 
            new { UserId = user.Id, token}, Request.Scheme);

        await emailService.SendEmailAsync(
            user.Email!,
            "Confirm your email",
            $"<p>To confirm your account, follow the <a href='{confirmLink}'>link</a>.</p>",
            ct
        );
        
        return RedirectToAction("RegisterConfirmation");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RegisterConfirmation() => View();

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }
        
        var result = await userManager.ConfirmEmailAsync(user, token);
        return View(result.Succeeded ? "EmailConfirmed" : "EmailConfirmationError");
    }
    
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}
