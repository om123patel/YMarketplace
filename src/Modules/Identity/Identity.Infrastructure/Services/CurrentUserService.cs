using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using Identity.Application.Services.Interfaces; // Ensure this is present for JwtRegisteredClaimNames

namespace Identity.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User
            => _httpContextAccessor.HttpContext?.User;

        private static string? GetClaimValue(ClaimsPrincipal? user, string claimType)
            => user?.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;

        public Guid UserId
        {
            get
            {
                var value = GetClaimValue(User, ClaimTypes.NameIdentifier)
                         ?? GetClaimValue(User, JwtRegisteredClaimNames.Sub);
                return Guid.TryParse(value, out var id) ? id : Guid.Empty;
            }
        }

        public string Email
            => GetClaimValue(User, ClaimTypes.Email)
            ?? GetClaimValue(User, JwtRegisteredClaimNames.Email)
            ?? string.Empty;

        public string Role
            => GetClaimValue(User, ClaimTypes.Role) ?? string.Empty;

        public bool IsAuthenticated
            => User?.Identity?.IsAuthenticated ?? false;

        public bool IsAdmin
            => Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

        public bool IsSeller
            => Role.Equals("Seller", StringComparison.OrdinalIgnoreCase);
    }
}
