using AdminPanel.ViewModels.Inventory;
using FluentValidation;

namespace AdminPanel.Validators.Inventory
{
    public class UpdateStockSettingsViewModelValidator
        : AbstractValidator<UpdateStockSettingsViewModel>
    {
        public UpdateStockSettingsViewModelValidator()
        {
            RuleFor(x => x.LowStockThreshold)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Low stock threshold cannot be negative.")
                .LessThanOrEqualTo(10_000)
                .WithMessage("Low stock threshold cannot exceed 10,000.");
        }
    }
}