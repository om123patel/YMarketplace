using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Application;
using Orders.Application.Interfaces;
using Orders.Infrastructure.Persistence;
using Orders.Infrastructure.Persistence.Repositories;
using Orders.Infrastructure.Persistence.UnitOfWork;

namespace Orders.Infrastructure
{
    public static class OrdersModule
    {
        public static IServiceCollection AddOrdersModule(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ── Application ──
            services.AddOrdersApplication();

            // ── DbContext ──
            services.AddDbContext<OrdersDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("EcommerceDB"),
                    sql => sql
                        .MigrationsHistoryTable("__MigrationsHistory", "orders")
                        .MigrationsAssembly(typeof(OrdersDbContext).Assembly.FullName)));

            // ── Unit of Work ──
            services.AddScoped<IOrdersUnitOfWork, OrdersUnitOfWork>();

            // ── Repositories ──
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IDisputeRepository, DisputeRepository>();

            return services;
        }
    }

}
