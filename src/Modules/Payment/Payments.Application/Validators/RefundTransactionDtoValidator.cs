using FluentValidation;
using Payments.Application.DTOs.Transactions;

namespace Payments.Application.Validators
{
    public class RefundTransactionDtoValidator
        : AbstractValidator<RefundTransactionDto>
    {
        public RefundTransactionDtoValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Refund amount must be greater than zero.");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Refund reason is required.")
                .MinimumLength(5).WithMessage("Please provide a meaningful reason.")
                .MaximumLength(500);
        }
    }
}