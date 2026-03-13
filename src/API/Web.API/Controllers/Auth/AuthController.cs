using Identity.Application.DTOs;
using Identity.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Extensions;

namespace Web.API.Controllers.Auth
{
    [Route("api/auth")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST api/auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            [FromBody] RegisterUserDto dto,
            CancellationToken ct)
        {
            var ipAddress = GetIpAddress();
            var result = await _authService.RegisterAsync(dto, ct);
            return HandleResult(result);
        }

        // POST api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromBody] LoginDto dto,
            CancellationToken ct)
        {
            dto.IpAddress = GetIpAddress();
            var result = await _authService.LoginAsync(dto, ct);
            return HandleResult(result);
        }

        // POST api/auth/refresh
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh(
            [FromBody] RefreshTokenDto dto,
            CancellationToken ct)
        {
            dto.IpAddress = GetIpAddress();
            var result = await _authService.RefreshTokenAsync(dto, ct);
            return HandleResult(result);
        }

        // POST api/auth/revoke
        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke(
            [FromBody] string refreshToken,
            CancellationToken ct)
        {
            var result = await _authService.RevokeTokenAsync(refreshToken, ct);
            return HandleResult(result);
        }

        // GET api/auth/me
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me(CancellationToken ct)
        {
            var userId = User.GetUserId();
            var result = await _authService.GetCurrentUserAsync(userId, ct);
            return HandleResult(result);
        }

        // PUT api/auth/profile
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateProfileDto dto,
            CancellationToken ct)
        {
            var userId = User.GetUserId();
            var result = await _authService.UpdateProfileAsync(userId, dto, ct);
            return HandleResult(result);
        }

        // PUT api/auth/change-password
        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordDto dto,
            CancellationToken ct)
        {
            var userId = User.GetUserId();
            var result = await _authService.ChangePasswordAsync(userId, dto, ct);
            return HandleResult(result);
        }

        private string GetIpAddress()
            => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
