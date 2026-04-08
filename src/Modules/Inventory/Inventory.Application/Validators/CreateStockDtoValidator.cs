using FluentValidation;
using Inventory.Application.DTOs;

namespace Inventory.Application.Validators
{
    public class CreateStockDtoValidator : AbstractValidator<CreateStockDto>
    {
        public CreateStockDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("ProductId is required.");

            RuleFor(x => x.InitialQuantity)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Initial quantity cannot be negative.")
                .LessThanOrEqualTo(1_000_000)
                .WithMessage("Initial quantity cannot exceed 1,000,000.");

            RuleFor(x => x.LowStockThreshold)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Low stock threshold cannot be negative.")
                .LessThanOrEqualTo(10_000)
                .WithMessage("Low stock threshold cannot exceed 10,000.");
        }
    }
}