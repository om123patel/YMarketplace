using AdminPanel.ViewModels.Inventory;
using FluentValidation;

namespace AdminPanel.Validators.Inventory
{
    public class SetQuantityViewModelValidator
        : AbstractValidator<SetQuantityViewModel>
    {
        public SetQuantityViewModelValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Quantity cannot be negative.")
                .LessThanOrEqualTo(1_000_000)
                .WithMessage("Quantity cannot exceed 1,000,000.");

            RuleFor(x => x.Note)
                .MaximumLength(500)
                .When(x => x.Note is not null);
        }
    }
}