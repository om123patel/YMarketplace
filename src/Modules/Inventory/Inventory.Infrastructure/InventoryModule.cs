using Inventory.Application;
using Inventory.Application.Interfaces;
using Inventory.Infrastructure.Persistence;
using Inventory.Infrastructure.Persistence.Repositories;
using Inventory.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Interfaces;

namespace Inventory.Infrastructure
{
    public static class InventoryModule
    {
        public static IServiceCollection AddInventoryModule(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddInventoryApplication();

            services.AddDbContext<InventoryDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("EcommerceDB"),
                    sql => sql
                        .MigrationsHistoryTable("__MigrationsHistory", "inventory")
                        .MigrationsAssembly(
                            typeof(InventoryDbContext).Assembly.FullName)));

            services.AddScoped<IUnitOfWork, InventoryUnitOfWork>();
            services.AddScoped<IStockRepository, StockRepository>();

            return services;
        }
    }
}