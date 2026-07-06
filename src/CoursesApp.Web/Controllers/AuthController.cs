using CoursesApp.Domain.Entities;
using CoursesApp.Web.DTOs.Auth;
using CoursesApp.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CoursesApp.Web.Controllers
{
    public class AuthController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IEmailService emailService) : Controller
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

            if (result.Succeeded)
            {
                var user = await userManager.FindByNameAsync(dto.Email);
                if (user is not null && !await userManager.IsEmailConfirmedAsync(user))
                {
                    await signInManager.SignOutAsync();
                    ModelState.AddModelError(string.Empty, "Please confirm your email before logging in.");
                    return View(dto);
                }
                return LocalRedirect(returnUrl ?? "/");
            }
            
            ModelState.AddModelError(string.Empty, "Invalid email or password");
            return View(dto);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterDto dto)
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
                $"<p>To confirm your account, follow the <a href='{confirmLink}'>link</a>.</p>"
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
}