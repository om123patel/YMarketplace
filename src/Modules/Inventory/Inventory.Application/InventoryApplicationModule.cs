using FluentValidation;
using Inventory.Application.DTOs;
using Inventory.Application.Mappings;
using Inventory.Application.Services;
using Inventory.Application.Services.Interface;
using Inventory.Application.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Application
{
    public static class InventoryApplicationModule
    {
        public static IServiceCollection AddInventoryApplication(
            this IServiceCollection services)
        {
            // Services
            services.AddScoped<IStockService, StockService>();

            // Validators — DTO level (service boundary)
            services.AddScoped<IValidator<AdjustStockDto>, AdjustStockDtoValidator>();
            services.AddScoped<IValidator<UpdateStockSettingsDto>,
                               UpdateStockSettingsDtoValidator>();
            services.AddScoped<IValidator<CreateStockDto>, CreateStockDtoValidator>();
            services.AddScoped<IValidator<ReserveStockDto>, ReserveStockDtoValidator>();

            // AutoMapper
            services.AddAutoMapper(cfg => { }, typeof(InventoryMappingProfile).Assembly);

            return services;
        }
    }
}