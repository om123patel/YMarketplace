using FluentValidation;
using Inventory.Application.DTOs;

namespace Inventory.Application.Validators
{
    /// <summary>
    /// Separate validator for set-exact-quantity operations —
    /// allows zero (wiping stock) but still has an upper bound.
    /// </summary>
    public class SetQuantityDtoValidator : AbstractValidator<AdjustStockDto>
    {
        public SetQuantityDtoValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Quantity cannot be negative.")
                .LessThanOrEqualTo(1_000_000)
                .WithMessage("Quantity cannot exceed 1,000,000.");

            RuleFor(x => x.Note)
                .MaximumLength(500)
                .WithMessage("Note cannot exceed 500 characters.")
                .When(x => x.Note is not null);
        }
    }
}