using FluentValidation;
using Payments.Application.DTOs.Payouts;

namespace Payments.Application.Validators
{
    public class RequestPayoutDtoValidator
        : AbstractValidator<RequestPayoutDto>
    {
        public RequestPayoutDtoValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Payout amount must be greater than zero.");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty().Length(3);

            // Either bank details or UPI must be provided
            RuleFor(x => x)
                .Must(x =>
                    !string.IsNullOrWhiteSpace(x.UpiId) ||
                    (!string.IsNullOrWhiteSpace(x.BankAccountNumber) &&
                     !string.IsNullOrWhiteSpace(x.BankIfscCode) &&
                     !string.IsNullOrWhiteSpace(x.BankAccountName)))
                .WithMessage("Either UPI ID or full bank account details are required.");

            RuleFor(x => x.BankIfscCode)
                .Matches(@"^[A-Z]{4}0[A-Z0-9]{6}$")
                .When(x => !string.IsNullOrWhiteSpace(x.BankIfscCode))
                .WithMessage("IFSC code format is invalid (e.g. SBIN0001234).");
        }
    }
}