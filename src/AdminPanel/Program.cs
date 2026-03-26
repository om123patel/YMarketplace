using AdminPanel.Services;
using AdminPanel.Services.Interfaces;
using Serilog;

namespace AdminPanel
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── Serilog ────────────────────────────────────────────────
            builder.Host.UseSerilog((ctx, config) =>
                config
                    .ReadFrom.Configuration(ctx.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console());

            // ──────────────────────────────────────────────────────────
            // SERVICES
            // ──────────────────────────────────────────────────────────
            var services = builder.Services;

            services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

            services.AddHttpContextAccessor();

            // ── Session (stores JWT in server-side session) ────────────
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(8);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                // FIX: Always — so session cookie works over plain HTTP in dev
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.Name = ".AdminPanel.Session";
            });

            // ── Cookie Auth ────────────────────────────────────────────
            services.AddAuthentication("AdminCookie")
                .AddCookie("AdminCookie", options =>
                {
                    options.LoginPath = "/auth/login";
                    options.LogoutPath = "/auth/logout";
                    // FIX: Send to a dedicated access-denied page, not back
                    // to login — redirecting to /auth/login when already on
                    // /auth/login was part of the redirect loop.
                    options.AccessDeniedPath = "/auth/access-denied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                    options.Cookie.Name = ".AdminPanel.Auth";
                    options.Cookie.HttpOnly = true;
                    // FIX: None (not Secure-only) so the cookie is sent
                    // over HTTP in development
                    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly",
                    policy => policy
                        .RequireAuthenticatedUser()
                        .RequireRole("Admin"));
            });

            // ── Typed HTTP Client ──────────────────────────────────────
            var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7276/";

            void ConfigureClient(HttpClient c)
            {
                c.BaseAddress = new Uri(apiBaseUrl);
                c.Timeout = TimeSpan.FromSeconds(30);
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            }


            // ── Application Services ───────────────────────────────────
            services.AddScoped<AuthTokenService>();
            services.AddHttpClient<IApiClient, ApiClient>(ConfigureClient);
            services.AddHttpClient<IProductApiClient, ProductApiClient>(ConfigureClient);
            services.AddHttpClient<ICategoryApiClient, CategoryApiClient>(ConfigureClient);
            services.AddHttpClient<IBrandApiClient, BrandApiClient>(ConfigureClient);
            services.AddHttpClient<ITagApiClient, TagApiClient>(ConfigureClient);
            services.AddHttpClient<IAttributeApiClient, AttributeApiClient>(ConfigureClient);
            services.AddHttpClient<ISellerApiClient, SellerApiClient>(ConfigureClient);


            // ──────────────────────────────────────────────────────────
            // BUILD APP
            // ──────────────────────────────────────────────────────────
            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
                // FIX: HTTPS redirect only in production — in development
                // localhost runs on plain HTTP, forcing HTTPS caused the loop
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Dashboard}/{action=Index}/{id?}");

            app.Logger.LogInformation(
                "AdminPanel started in {Environment} mode",
                app.Environment.EnvironmentName);

            app.Run();
        }
    }
}