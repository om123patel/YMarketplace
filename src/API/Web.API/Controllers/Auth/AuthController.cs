using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Extensions;

namespace Web.API.Controllers.Identity
{
    [Route("api/auth")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
            => _authService = authService;

        // POST api/auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            [FromBody] RegisterDto dto, CancellationToken ct)
        {
            var result = await _authService.RegisterAsync(dto, ct);
            return HandleResult(result);
        }

        // POST api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromBody] LoginDto dto, CancellationToken ct)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _authService.LoginAsync(dto, ip, ct);
            return HandleResult(result);
        }

        // POST api/auth/refresh
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh(
            [FromBody] RefreshRequestDto dto, CancellationToken ct)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _authService.RefreshTokenAsync(dto.RefreshToken, ip, ct);
            return HandleResult(result);
        }

        // POST api/auth/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(
            [FromBody] RefreshRequestDto dto, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var result = await _authService.RevokeTokenAsync(
                dto.RefreshToken, userId, ct);
            return HandleResult(result);
        }

        // GET api/auth/me
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me(
            [FromServices] IUserService userService, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var result = await userService.GetByIdAsync(userId, ct);
            return HandleResult(result);
        }
    }

    public class RefreshRequestDto
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}