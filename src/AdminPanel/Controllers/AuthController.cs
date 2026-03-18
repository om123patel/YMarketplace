using AdminPanel.Services;
using AdminPanel.Services.Interfaces;
using AdminPanel.ViewModels.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdminPanel.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly IApiClient _api;
        private readonly AuthTokenService _tokenService;

        public AuthController(IApiClient api, AuthTokenService tokenService)
        {
            _api = api;
            _tokenService = tokenService;
        }

        // GET /auth/login
        [HttpGet("login")]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl, string? error)
        {
            // Already logged in → go straight to dashboard
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            var vm = new LoginViewModel { ReturnUrl = returnUrl };

            if (error == "access_denied")
                vm.Error = "Access denied. Admin accounts only.";

            return View(vm);
        }

        // POST /auth/login
        // POST /auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _api.LoginAsync(model.Email, model.Password);

            if (result?.Success != true || result.Data is null)
            {
                model.Error = result?.Error ?? "Login failed. Please check your credentials.";
                return View(model);
            }

            var data = result.Data;

            if (!data.User.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                model.Error = "Access denied. Admin accounts only.";
                return View(model);
            }

            // 1. Store JWT in server-side session
            _tokenService.StoreTokens(data.AccessToken, data.RefreshToken);

            // 2. Also embed tokens in claims so they survive session loss
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, data.User.Id.ToString()),
                new(ClaimTypes.Email,          data.User.Email),
                new(ClaimTypes.Name,           data.User.FullName),
                new(ClaimTypes.Role,           data.User.Role),
                // Embed JWT directly in cookie claim as fallback
                new("access_token",  data.AccessToken),
                new("refresh_token", data.RefreshToken),
            };

            var identity = new ClaimsIdentity(claims, "AdminCookie");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("AdminCookie", principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            var returnUrl = model.ReturnUrl;
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard");
        }

        // POST /auth/logout
        [HttpPost("logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            _tokenService.ClearTokens();
            await HttpContext.SignOutAsync("AdminCookie");
            return RedirectToAction("Login");
        }

        // GET /auth/access-denied
        // Shown when an authenticated user hits a resource they don't have
        // permission for (e.g. a non-Admin that somehow gets a valid cookie).
        // This prevents AccessDeniedPath = /auth/login from creating a loop
        // for users who ARE authenticated but lack the Admin role.
        [HttpGet("access-denied")]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            // If already signed in but wrong role, sign them out cleanly
            if (User.Identity?.IsAuthenticated == true)
            {
                _tokenService.ClearTokens();
                HttpContext.SignOutAsync("AdminCookie").GetAwaiter().GetResult();
            }

            return RedirectToAction("Login", new { error = "access_denied" });
        }
    }
}