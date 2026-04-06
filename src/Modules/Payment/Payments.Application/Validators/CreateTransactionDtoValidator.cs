using FluentValidation;
using Payments.Application.DTOs.Transactions;

namespace Payments.Application.Validators
{
    public class CreateTransactionDtoValidator
        : AbstractValidator<CreateTransactionDto>
    {
        private static readonly string[] AllowedMethods =
            ["Card", "UPI", "NetBanking", "Wallet", "COD"];

        public CreateTransactionDtoValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage("OrderId is required.");

            RuleFor(x => x.BuyerId)
                .NotEmpty().WithMessage("BuyerId is required.");

            RuleFor(x => x.SellerId)
                .NotEmpty().WithMessage("SellerId is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty().Length(3)
                .WithMessage("Currency code must be 3 characters.");

            RuleFor(x => x.Method)
                .Must(m => AllowedMethods.Contains(m))
                .WithMessage($"Method must be one of: {string.Join(", ", AllowedMethods)}.");
        }
    }
}