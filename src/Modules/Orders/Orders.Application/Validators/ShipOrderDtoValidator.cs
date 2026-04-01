using FluentValidation;
using Orders.Application.DTOs.Orders;

namespace Orders.Application.Validators
{
    public class ShipOrderDtoValidator : AbstractValidator<ShipOrderDto>
    {
        public ShipOrderDtoValidator()
        {
            RuleFor(x => x.TrackingNumber).NotEmpty()
                .MaximumLength(100).WithMessage("Tracking number is required.");
            RuleFor(x => x.Carrier).NotEmpty()
                .MaximumLength(100).WithMessage("Carrier name is required.");
            RuleFor(x => x.TrackingUrl).MaximumLength(500)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrWhiteSpace(x.TrackingUrl))
                .WithMessage("Tracking URL must be a valid URL.");
        }
    }

}
