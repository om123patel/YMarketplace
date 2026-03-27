using Identity.Application;
using Identity.Application.Interfaces;
using Identity.Application.Services.Interfaces;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence.Repositories;
using Identity.Infrastructure.Persistence.UnitOfWork;
using Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shared.Application.Interfaces;
using System.Security.Claims;
using System.Text;


namespace Identity.Infrastructure;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── 1. Application Layer ───────────────────────────────
        services.AddIdentityApplication();

        // ── 2. DbContext ───────────────────────────────────────
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("EcommerceDB"),
                sql =>
                {
                    sql.MigrationsHistoryTable(
                        "__MigrationsHistory", "identity");
                    sql.MigrationsAssembly(
                        typeof(IdentityDbContext).Assembly.FullName);
                    sql.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                }));

        // ── 3. Unit of Work ────────────────────────────────────
        services.AddScoped<IUnitOfWork, IdentityUnitOfWork>();

        // ── 4. Repositories ────────────────────────────────────
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ISellerRepository, SellerRepository>();

        // ── 5. Infrastructure Services ─────────────────────────
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // ── 6. JWT Token Validation Rules ─────────────────────
        // Infrastructure owns ONLY token validation parameters.
        // How the HTTP pipeline responds to auth failures
        // is configured in the API project via AddJwtBearerEvents().
        services.AddJwtTokenValidation(configuration);

        // ── 7. Authorization Policies ──────────────────────────
        services.AddAuthorizationPolicies();

        return services;
    }

    // ══════════════════════════════════════════════════════════
    // PRIVATE — Token Validation Only
    // Pure cryptographic config. Zero HTTP pipeline behavior.
    // ══════════════════════════════════════════════════════════
    private static IServiceCollection AddJwtTokenValidation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("JwtSettings");

        var secretKey = jwtSection["SecretKey"]
            ?? throw new InvalidOperationException(
                "JwtSettings:SecretKey is not configured.");

        var issuer = jwtSection["Issuer"]
            ?? throw new InvalidOperationException(
                "JwtSettings:Issuer is not configured.");

        var audience = jwtSection["Audience"]
            ?? throw new InvalidOperationException(
                "JwtSettings:Audience is not configured.");

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // true in production
                options.SaveToken = true;

                // Infrastructure owns ONLY these validation parameters.
                // This is pure cryptographic config — no HTTP behavior.
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = signingKey,

                        ClockSkew = TimeSpan.Zero,

                        // Maps JWT "sub"  → ClaimTypes.NameIdentifier
                        // Maps JWT "role" → ClaimTypes.Role
                        // So CurrentUserService only needs ClaimTypes
                        NameClaimType = ClaimTypes.NameIdentifier,
                        RoleClaimType = ClaimTypes.Role
                    };

                // ── NO JwtBearerEvents here ──
                // OnAuthenticationFailed → API project
                // OnChallenge            → API project
                // OnForbidden            → API project
            });

        return services;
    }

    // ══════════════════════════════════════════════════════════
    // PRIVATE — Authorization Policies
    // Pure wiring — no HTTP behavior, no response formatting.
    // ══════════════════════════════════════════════════════════
    private static IServiceCollection AddAuthorizationPolicies(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly",
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole("Admin"));

            options.AddPolicy("SellerOnly",
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole("Seller"));

            options.AddPolicy("BuyerOnly",
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole("Buyer"));

            options.AddPolicy("AdminOrSeller",
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole("Admin", "Seller"));

            options.AddPolicy("AdminOrBuyer",
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole("Admin", "Buyer"));

            options.AddPolicy("AnyRole",
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole("Admin", "Seller", "Buyer"));
        });

        return services;
    }
}
