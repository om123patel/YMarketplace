using Catalog.Application.DTOs.Products;
using FluentValidation;

namespace Catalog.Application.Validators.Product
{
    public class RejectProductDtoValidator : AbstractValidator<RejectProductDto>
    {
        public RejectProductDtoValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Rejection reason is required.")
                .MinimumLength(10).WithMessage("Please provide a meaningful rejection reason (min 10 characters).")
                .MaximumLength(1000).WithMessage("Rejection reason cannot exceed 1000 characters.");
        }
    }

}
