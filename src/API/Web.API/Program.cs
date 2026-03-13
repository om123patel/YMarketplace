using Catalog.Infrastructure;
using Identity.Infrastructure;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Serilog;
using Shared.Infrastructure;
using Web.API.Extensions;
using Web.API.Middleware;
namespace Web.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ══════════════════════════════════════════════════════
            // SERILOG
            // ══════════════════════════════════════════════════════
            builder.Host.UseSerilog((ctx, config) =>
                config
                    .ReadFrom.Configuration(ctx.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File(
                        path: "logs/api-.log",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30));

            // ══════════════════════════════════════════════════════
            // SERVICES
            // ══════════════════════════════════════════════════════
            var services = builder.Services;
            var configuration = builder.Configuration;

            // ── Core ──
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy =
                        System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.DefaultIgnoreCondition =
                        System.Text.Json.Serialization.JsonIgnoreCondition
                            .WhenWritingNull;
                });

            services.AddEndpointsApiExplorer();
            services.AddHttpContextAccessor();

            // ── CORS ──
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());

                options.AddPolicy("Production", policy =>
                    policy
                        .WithOrigins(
                            configuration
                                .GetSection("AllowedOrigins")
                                .Get<string[]>() ?? [])
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            // ── Swagger ──
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Ecommerce Marketplace API",
                    Version = "v1",
                    Description = "API for Ecommerce Marketplace Platform"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
            });

            // ── Modules ──
            services.AddSharedInfrastructure(configuration);
            services.AddIdentityModule(configuration);
            services.AddCatalogModule(configuration);

            // ── JWT Bearer Events ──────────────────────────────────
            // API project owns ALL HTTP pipeline behavior.
            // Infrastructure only configured token validation rules.
            // Here we configure how the pipeline responds to auth events.
            services.ConfigureJwtBearerEvents();

            // ══════════════════════════════════════════════════════
            // BUILD APP
            // ══════════════════════════════════════════════════════
            var app = builder.Build();

            // ── Middleware Pipeline ──
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint(
                        "/swagger/v1/swagger.json",
                        "Ecommerce Marketplace API v1");
                    options.RoutePrefix = "swagger";
                });
            }

            app.UseHttpsRedirection();
            app.UseCors(app.Environment.IsDevelopment()
                ? "AllowAll"
                : "Production");

            // Serve uploaded files as static files (local storage dev only)
            var uploadPath = configuration["Storage:Local:UploadPath"] ?? "uploads";
            Directory.CreateDirectory(uploadPath);

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), uploadPath)),
                RequestPath = "/uploads"
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Logger.LogInformation(
                "Ecommerce Marketplace API started in {Environment} mode",
                app.Environment.EnvironmentName);

            app.Run();
        }
    }
}
