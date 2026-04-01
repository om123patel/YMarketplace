using FluentValidation;
using Orders.Application.DTOs.Orders;

namespace Orders.Application.Validators
{
    public class PlaceOrderDtoValidator : AbstractValidator<PlaceOrderDto>
    {
        public PlaceOrderDtoValidator()
        {
            RuleFor(x => x.StoreId).NotEmpty().WithMessage("StoreId is required.");
            RuleFor(x => x.SellerId).NotEmpty().WithMessage("SellerId is required.");
            RuleFor(x => x.CurrencyCode).NotEmpty().Length(3)
                .WithMessage("Currency code must be exactly 3 characters.");
            RuleFor(x => x.ShippingAmount).GreaterThanOrEqualTo(0)
                .WithMessage("Shipping amount cannot be negative.");
            RuleFor(x => x.Items).NotEmpty()
                .WithMessage("An order must contain at least one item.");
            RuleForEach(x => x.Items).SetValidator(new PlaceOrderItemDtoValidator());
        }
    }

}
