using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Payments.Application.Mappings;
using Payments.Application.Services;
using Payments.Application.Services.Interface;
using Payments.Application.Validators;

namespace Payments.Application
{
    public static class PaymentsApplicationModule
    {
        public static IServiceCollection AddPaymentsApplication(
            this IServiceCollection services)
        {
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IPayoutService, PayoutService>();
            services.AddScoped<ICommissionRuleService, CommissionRuleService>();
            services.AddScoped<ICheckoutService, CheckoutService>();

            services.AddAutoMapper(
                cfg => { }, typeof(PaymentsMappingProfile).Assembly);

            services.AddValidatorsFromAssembly(
                typeof(CreateTransactionDtoValidator).Assembly);

            return services;
        }
    }
}