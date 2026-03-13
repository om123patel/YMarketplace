using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text.Json;
using Web.API.Models;

namespace Web.API.Extensions
{
    public static class JwtBearerEventsExtensions
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static IServiceCollection ConfigureJwtBearerEvents(
            this IServiceCollection services)
        {
            // Post-configure runs AFTER AddJwtBearer in IdentityModule
            // so we are adding behavior on top of the validation rules
            // without touching Infrastructure at all
            services.PostConfigure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.Events = new JwtBearerEvents
                    {
                        // ── Token expired or invalid ──────────────
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception is
                                Microsoft.IdentityModel.Tokens
                                    .SecurityTokenExpiredException)
                            {
                                // Tell client specifically that
                                // token expired vs completely invalid
                                context.Response.Headers.Append(
                                    "Token-Expired", "true");
                            }

                            return Task.CompletedTask;
                        },

                        // ── No token or invalid token (401) ───────
                        OnChallenge = context =>
                        {
                            // Suppress default ASP.NET Core 401 behavior
                            // Return clean JSON response instead
                            context.HandleResponse();

                            if (context.Response.HasStarted)
                                return Task.CompletedTask;

                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";

                            var isExpired = context.Response.Headers
                                .ContainsKey("Token-Expired");

                            var response = ApiResponse.Fail(
                                error: isExpired
                                    ? "Your session has expired. Please login again."
                                    : "Unauthorized. Please provide a valid token.",
                                errorCode: isExpired
                                    ? "TOKEN_EXPIRED"
                                    : "UNAUTHORIZED",
                                statusCode: 401);

                            return context.Response.WriteAsync(
                                JsonSerializer.Serialize(
                                    response, SerializerOptions));
                        },

                        // ── Authenticated but wrong role (403) ────
                        OnForbidden = context =>
                        {
                            if (context.Response.HasStarted)
                                return Task.CompletedTask;

                            context.Response.StatusCode = 403;
                            context.Response.ContentType = "application/json";

                            var response = ApiResponse.Fail(
                                error: "Forbidden. You do not have permission " +
                                       "to access this resource.",
                                errorCode: "FORBIDDEN",
                                statusCode: 403);

                            return context.Response.WriteAsync(
                                JsonSerializer.Serialize(
                                    response, SerializerOptions));
                        }
                    };
                });

            return services;
        }
    }

}
