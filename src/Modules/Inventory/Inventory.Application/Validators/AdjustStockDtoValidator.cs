using FluentValidation;
using Inventory.Application.DTOs;

namespace Inventory.Application.Validators
{
    public class AdjustStockDtoValidator : AbstractValidator<AdjustStockDto>
    {
        public AdjustStockDtoValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero.")
                .LessThanOrEqualTo(100_000)
                .WithMessage("Quantity cannot exceed 100,000 in a single adjustment.");

            RuleFor(x => x.Note)
                .MaximumLength(500)
                .WithMessage("Note cannot exceed 500 characters.")
                .When(x => x.Note is not null);
        }
    }
}