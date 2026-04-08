using FluentValidation;
using Inventory.Application.DTOs;

namespace Inventory.Application.Validators
{
    public class ReserveStockDtoValidator : AbstractValidator<ReserveStockDto>
    {
        public ReserveStockDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty()
                .WithMessage("ProductId is required.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Reservation quantity must be greater than zero.")
                .LessThanOrEqualTo(100_000)
                .WithMessage("Reservation quantity cannot exceed 100,000.");
        }
    }
}