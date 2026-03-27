using FluentValidation;
using Identity.Application.Interfaces;
using Identity.Application.Mappings;
using Identity.Application.Services;
using Identity.Application.Services.Interfaces;
using Identity.Application.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Application
{
    public static class IdentityApplicationModule
    {
        public static IServiceCollection AddIdentityApplication(
            this IServiceCollection services)
        {
            // ── Services ──
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ISellerService, SellerService>();

            // ── AutoMapper ──
            services.AddAutoMapper(cfg => { }, typeof(IdentityMappingProfile).Assembly);

            // ── FluentValidation ──
            services.AddValidatorsFromAssembly(
                typeof(RegisterUserDtoValidator).Assembly);

            return services;
        }
    }
}
