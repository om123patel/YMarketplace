using Catalog.Application.DTOs.Products;
using FluentValidation;

namespace Catalog.Application.Validators.Product
{
    public class ProductAttributeDtoValidator : AbstractValidator<ProductAttributeDto>
    {
        public ProductAttributeDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Attribute name is required.")
                .MaximumLength(100).WithMessage("Attribute name cannot exceed 100 characters.");

            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("Attribute value is required.")
                .MaximumLength(200).WithMessage("Attribute value cannot exceed 200 characters.");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Sort order cannot be negative.");
        }
    }


}
