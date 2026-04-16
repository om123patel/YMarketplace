// src/API/Web.API/Controllers/Customer/CustomerAuthController.cs
using Identity.Application.DTOs.Customer;
using Identity.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Extensions;

namespace Web.API.Controllers.Customer
{
    [Route("api/customer/auth")]
    public class CustomerAuthController : BaseController
    {
        private readonly ICustomerAuthService _customerAuthService;

        public CustomerAuthController(ICustomerAuthService customerAuthService)
            => _customerAuthService = customerAuthService;

        /// <summary>
        /// Register a new customer account.
        /// Role is always Buyer — not exposed in the request body.
        /// </summary>
        // POST /api/customer/auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            [FromBody] CustomerRegisterDto dto,
            CancellationToken ct)
        {
            var result = await _customerAuthService.RegisterAsync(dto, ct);
            return HandleResult(result);
        }

        /// <summary>
        /// Authenticate as a customer and receive JWT + refresh token.
        /// Only Buyer accounts are accepted.
        /// </summary>
        // POST /api/customer/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromBody] CustomerLoginDto dto,
            CancellationToken ct)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _customerAuthService.LoginAsync(dto, ip, ct);
            return HandleResult(result);
        }

        /// <summary>
        /// Invalidate the customer's current refresh token.
        /// The access token expires naturally (use client-side token removal for immediate effect).
        /// </summary>
        // POST /api/customer/auth/logout
        [HttpPost("logout")]
        [Authorize(Policy = "BuyerOnly")]
        public async Task<IActionResult> Logout(
            [FromBody] CustomerLogoutDto dto,
            CancellationToken ct)
        {
            var userId = User.GetUserId();
            var result = await _customerAuthService.LogoutAsync(dto.RefreshToken, userId, ct);
            return HandleResult(result);
        }

        /// <summary>
        /// Get the currently authenticated customer's profile.
        /// </summary>
        // GET /api/customer/auth/me
        [HttpGet("me")]
        [Authorize(Policy = "BuyerOnly")]
        public async Task<IActionResult> GetMe(CancellationToken ct)
        {
            var userId = User.GetUserId();
            var result = await _customerAuthService.GetMeAsync(userId, ct);
            return HandleResult(result);
        }
    }
}