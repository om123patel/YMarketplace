using Catalog.Application.Mappings;
using Catalog.Application.Services;
using Catalog.Application.Services.Interface;
using Catalog.Application.Validators.Product;
using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Application
{
    public static class CatalogApplicationModule
    {
        public static IServiceCollection AddCatalogApplication(
            this IServiceCollection services)
        {
            // ── Services ──
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IBrandService, BrandService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<IAttributeTemplateService, AttributeTemplateService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ISellerProductService, SellerProductService>();

            // ── AutoMapper ──
            // Scans Catalog.Application assembly for all Profile classes
            services.AddAutoMapper(cfg => { }, typeof(CatalogMappingProfile).Assembly);

            // ── FluentValidation ──
            // Scans Catalog.Application assembly and registers all validators
            services.AddValidatorsFromAssembly(
                typeof(CreateProductDtoValidator).Assembly);

            return services;
        }
    }
}
