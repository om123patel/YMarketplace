using FluentValidation;
using Inventory.Application.DTOs;

namespace Inventory.Application.Validators
{
    public class UpdateStockSettingsDtoValidator
        : AbstractValidator<UpdateStockSettingsDto>
    {
        public UpdateStockSettingsDtoValidator()
        {
            RuleFor(x => x.LowStockThreshold)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Low stock threshold cannot be negative.")
                .LessThanOrEqualTo(10_000)
                .WithMessage("Low stock threshold cannot exceed 10,000.");
        }
    }
}