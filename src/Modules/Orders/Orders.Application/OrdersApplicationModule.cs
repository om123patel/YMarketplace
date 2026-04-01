using Orders.Application.Mappings;
using Orders.Application.Services;
using Orders.Application.Services.Interface;
using Orders.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;


namespace Orders.Application
{
    public static class OrdersApplicationModule
    {
        public static IServiceCollection AddOrdersApplication(
            this IServiceCollection services)
        {
            // ── Services ──
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IDisputeService, DisputeService>();

            // ── AutoMapper ──
            services.AddAutoMapper(cfg => { }, typeof(OrdersMappingProfile).Assembly);

            // ── FluentValidation ──
            services.AddValidatorsFromAssembly(typeof(PlaceOrderDtoValidator).Assembly);

           

            return services;
        }
    }

}
