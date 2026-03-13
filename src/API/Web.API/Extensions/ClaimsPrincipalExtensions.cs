using System.Security.Claims;

namespace Web.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? user.FindFirstValue("sub");

            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        public static string GetEmail(this ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.Email)
            ?? user.FindFirstValue("email")
            ?? string.Empty;

        public static string GetRole(this ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        public static bool IsAdmin(this ClaimsPrincipal user)
            => user.IsInRole("Admin");

        public static bool IsSeller(this ClaimsPrincipal user)
            => user.IsInRole("Seller");
    }

}
