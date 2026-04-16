using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payments.Application;
using Payments.Application.Interfaces;
using Payments.Infrastructure.Gateways;
using Payments.Infrastructure.Gateways.Models;
using Payments.Infrastructure.Gateways.Payments.Infrastructure.Gateways;
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


            // ── Gateway Options ──
            services.Configure<RazorpayOptions>(
                configuration.GetSection(RazorpayOptions.Section));
            services.Configure<CashfreeOptions>(
                configuration.GetSection(CashfreeOptions.Section));
            services.Configure<PayUOptions>(
                configuration.GetSection(PayUOptions.Section));
            services.Configure<PhonePeOptions>(
                configuration.GetSection(PhonePeOptions.Section));
            services.Configure<PaytmOptions>(
                configuration.GetSection(PaytmOptions.Section));

            // ── HTTP Clients for gateways that need them ──
            services.AddHttpClient("Razorpay");
            services.AddHttpClient("Cashfree");
            services.AddHttpClient("PhonePe");
            services.AddHttpClient("Paytm");

            // ── Gateway Implementations ──
            services.AddScoped<IPaymentGateway, RazorpayGateway>();
            services.AddScoped<IPaymentGateway, CashfreeGateway>();
            services.AddScoped<IPaymentGateway, PayUGateway>();
            services.AddScoped<IPaymentGateway, PhonePeGateway>();
            services.AddScoped<IPaymentGateway, PaytmGateway>();

            // ── Gateway Factory ──
            services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();

            return services;

           
        }
    }
}