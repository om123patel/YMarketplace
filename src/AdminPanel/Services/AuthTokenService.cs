using System.Security.Claims;

namespace AdminPanel.Services
{
    public class AuthTokenService
    {
        private const string TokenKey = "jwt_access_token";
        private const string RefreshKey = "jwt_refresh_token";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthTokenService(IHttpContextAccessor httpContextAccessor)
            => _httpContextAccessor = httpContextAccessor;

        private HttpContext Context
            => _httpContextAccessor.HttpContext!;

        // ── Write ──────────────────────────────────────────────
        public void StoreTokens(string accessToken, string refreshToken)
        {
            Context.Session.SetString(TokenKey, accessToken);
            Context.Session.SetString(RefreshKey, refreshToken);
        }

        // ── Read — session first, claim fallback ───────────────
        public string? GetAccessToken()
        {
            // Primary: session
            var token = Context.Session.GetString(TokenKey);
            if (!string.IsNullOrWhiteSpace(token))
                return token;

            // Fallback: cookie claim (survives session expiry / server restart)
            token = Context.User.FindFirstValue("access_token");
            if (!string.IsNullOrWhiteSpace(token))
            {
                // Rehydrate session so subsequent calls hit session again
                var refresh = Context.User.FindFirstValue("refresh_token") ?? "";
                StoreTokens(token, refresh);
                return token;
            }

            return null;
        }

        public string? GetRefreshToken()
        {
            var token = Context.Session.GetString(RefreshKey);
            if (!string.IsNullOrWhiteSpace(token))
                return token;

            return Context.User.FindFirstValue("refresh_token");
        }

        // ── Clear ──────────────────────────────────────────────
        public void ClearTokens()
        {
            Context.Session.Remove(TokenKey);
            Context.Session.Remove(RefreshKey);
            Context.Session.Clear();
        }
    }

}
