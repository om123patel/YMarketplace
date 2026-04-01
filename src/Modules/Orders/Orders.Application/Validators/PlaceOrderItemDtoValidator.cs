using FluentValidation;
using Orders.Application.DTOs.Orders;

namespace Orders.Application.Validators
{
    public class PlaceOrderItemDtoValidator : AbstractValidator<PlaceOrderItemDto>
    {
        public PlaceOrderItemDtoValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty();
            RuleFor(x => x.ProductName).NotEmpty().MaximumLength(300);
            RuleFor(x => x.Quantity).GreaterThan(0)
                .WithMessage("Item quantity must be greater than zero.");
            RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0)
                .WithMessage("Unit price cannot be negative.");
        }
    }

}
