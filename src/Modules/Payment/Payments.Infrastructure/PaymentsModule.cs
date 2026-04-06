using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payments.Application;
using Payments.Application.Interfaces;
using Payments.Infrastructure.Persistence;
using Payments.Infrastructure.Persistence.Repositories;
using Payments.Infrastructure.Persistence.UnitOfWork;

namespace Payments.Infrastructure
{
    public static class PaymentsModule
    {
        public static IServiceCollection AddPaymentsModule(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddPaymentsApplication();

            services.AddDbContext<PaymentsDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("EcommerceDB"),
                    sql => sql
                        .MigrationsHistoryTable("__MigrationsHistory", "payments")
                        .MigrationsAssembly(
                            typeof(PaymentsDbContext).Assembly.FullName)));

            services.AddScoped<IPaymentsUnitOfWork, PaymentsUnitOfWork>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IPayoutRepository, PayoutRepository>();
            services.AddScoped<ICommissionRuleRepository, CommissionRuleRepository>();

            return services;
        }
    }
}