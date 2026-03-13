using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Interfaces;
using Shared.Infrastructure.EventBus;
using Shared.Infrastructure.Storage;

namespace Shared.Infrastructure
{

    public static class SharedInfrastructureModule
    {
        public static IServiceCollection AddSharedInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ── Event Bus ──
            services.AddScoped<IEventBus, InMemoryEventBus>();

            // ── Storage Service ──
            // Swap provider by changing "Storage:Provider" in appsettings
            var storageProvider = configuration["Storage:Provider"] ?? "Local";

            switch (storageProvider)
            {
                case "Azure":
                    services.AddScoped<IStorageService,
                        AzureBlobStorageService>();
                    break;

                case "R2":
                    services.AddScoped<IStorageService,
                        CloudflareR2StorageService>();
                    break;

                default: // "Local"
                    services.AddScoped<IStorageService,
                        LocalStorageService>();
                    break;
            }

            return services;
        }
    }

}
