using AdminPanel.Services;
using AdminPanel.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace AdminPanel.Middleware
{
    /// <summary>
    /// Intercepts 401 Unauthorized responses that originate from an expired
    /// JWT access token.  When a valid refresh token is present in the session
    /// the middleware:
    ///   1. Calls the API refresh endpoint.
    ///   2. Stores the new token pair in the session.
    ///   3. Re-issues the cookie auth sign-in (preserving the original claims).
    ///   4. Retries the original request once with the new access token.
    ///
    /// If the refresh itself fails (token expired / revoked) the user is sent
    /// to the login page with all session state cleared.
    ///
    /// Registration order in Program.cs:
    ///   app.UseSession();
    ///   app.UseAuthentication();
    ///   app.UseAuthorization();
    ///   app.UseMiddleware&lt;TokenRefreshMiddleware&gt;();   ← AFTER auth
    /// </summary>
    public class TokenRefreshMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenRefreshMiddleware> _logger;

        public TokenRefreshMiddleware(
            RequestDelegate next,
            ILogger<TokenRefreshMiddleware> logger)
        {
            _next   = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IApiClient api,
            AuthTokenService tokens)
        {
            // Run the rest of the pipeline first
            await _next(context);

            // Only act on 401 responses for authenticated (cookie) sessions.
            // Skip auth endpoints to avoid infinite loops.
            if (context.Response.StatusCode != StatusCodes.Status401Unauthorized)
                return;

            if (IsAuthRoute(context.Request.Path))
                return;

            // Must have a refresh token stored from the original login
            var refreshToken = tokens.GetRefreshToken();
            if (string.IsNullOrEmpty(refreshToken))
            {
                await RedirectToLogin(context);
                return;
            }

            _logger.LogInformation(
                "Access token expired for {Path} — attempting silent refresh",
                context.Request.Path);

            // ── Attempt refresh ──────────────────────────────────
            var refreshResult = await api.RefreshTokenAsync(refreshToken);

            if (refreshResult?.Success != true || refreshResult.Data is null)
            {
                _logger.LogWarning(
                    "Token refresh failed ({Error}) — redirecting to login",
                    refreshResult?.Error ?? "null response");

                tokens.ClearTokens();
                await context.SignOutAsync("AdminCookie");
                await RedirectToLogin(context);
                return;
            }

            // ── Store new tokens ─────────────────────────────────
            tokens.StoreTokens(
                refreshResult.Data.AccessToken,
                refreshResult.Data.RefreshToken);

            // ── Re-issue the cookie (keep existing claims) ───────
            // The cookie may have expired or not yet been updated in this
            // response, so we re-sign with the current principal's claims.
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var authProps = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc   = DateTimeOffset.UtcNow.AddHours(8)
                };
                await context.SignInAsync("AdminCookie", context.User, authProps);
            }

            _logger.LogInformation("Token refresh succeeded — retrying request");

            // ── Retry: redirect back to the same URL ─────────────
            // We cannot replay the request body (it has already been consumed),
            // so we issue a redirect for GET requests and return a 401 for
            // POST/PATCH/DELETE so the calling JS or form can handle it.
            if (HttpMethods.IsGet(context.Request.Method))
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status302Found;
                context.Response.Headers.Location =
                    context.Request.Path + context.Request.QueryString;
            }
            // Non-GET: leave as 401 — the client (form/JS) must retry.
            // The new tokens are now in the session, so the next request
            // made by the browser will succeed automatically.
        }

        // ── Helpers ──────────────────────────────────────────────
        private static bool IsAuthRoute(PathString path)
            => path.StartsWithSegments("/auth", StringComparison.OrdinalIgnoreCase);

        private static Task RedirectToLogin(HttpContext context)
        {
            var returnUrl = context.Request.Path + context.Request.QueryString;
            context.Response.Clear();
            context.Response.StatusCode  = StatusCodes.Status302Found;
            context.Response.Headers.Location =
                $"/auth/login?returnUrl={Uri.EscapeDataString(returnUrl)}";
            return Task.CompletedTask;
        }
    }
}
