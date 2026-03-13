using Catalog.Application;
using Catalog.Application.Interfaces;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Repositories;
using Catalog.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Interfaces;

namespace Catalog.Infrastructure
{
    public static class CatalogModule
    {
        public static IServiceCollection AddCatalogModule(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ── Application Layer Registration ──
            // Infrastructure calls Application registration
            // so API only needs to call AddCatalogModule — one entry point
            services.AddCatalogApplication();

            // ── DbContext ──
            services.AddDbContext<CatalogDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("EcommerceDB"),
                    sql => sql
                        .MigrationsHistoryTable("__MigrationsHistory", "catalog")
                        .MigrationsAssembly(
                            typeof(CatalogDbContext).Assembly.FullName)));

            // ── Unit of Work ──
            services.AddScoped<IUnitOfWork, CatalogUnitOfWork>();

            // ── Repositories ──
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IBrandRepository, BrandRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IAttributeTemplateRepository, AttributeTemplateRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductStatusHistoryRepository,
                ProductStatusHistoryRepository>();

            return services;
        }
    }

}
