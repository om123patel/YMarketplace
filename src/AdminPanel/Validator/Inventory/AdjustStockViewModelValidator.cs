using AdminPanel.ViewModels.Inventory;
using FluentValidation;

namespace AdminPanel.Validators.Inventory
{
    public class AdjustStockViewModelValidator
        : AbstractValidator<AdjustStockViewModel>
    {
        public AdjustStockViewModelValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero.")
                .LessThanOrEqualTo(100_000)
                .WithMessage("Quantity cannot exceed 100,000 in one adjustment.");

            RuleFor(x => x.Note)
                .MaximumLength(500)
                .WithMessage("Note cannot exceed 500 characters.")
                .When(x => x.Note is not null);
        }
    }
}