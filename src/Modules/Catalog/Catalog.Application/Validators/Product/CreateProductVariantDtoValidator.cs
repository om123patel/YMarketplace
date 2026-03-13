using Catalog.Application.DTOs;
using FluentValidation;

namespace Catalog.Application.Validators.Product
{
    public class CreateProductVariantDtoValidator
    : AbstractValidator<CreateProductVariantDto>
    {
        public CreateProductVariantDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Variant name is required.")
                .MaximumLength(200).WithMessage("Variant name cannot exceed 200 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Variant price must be greater than zero.");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty().WithMessage("Currency code is required.")
                .Length(3).WithMessage("Currency code must be exactly 3 characters (e.g. USD).");

            RuleFor(x => x.CompareAtPrice)
                .GreaterThanOrEqualTo(x => x.Price)
                .When(x => x.CompareAtPrice.HasValue)
                .WithMessage("Compare at price must be greater than or equal to price.");

            RuleFor(x => x.CostPrice)
                .GreaterThanOrEqualTo(0)
                .When(x => x.CostPrice.HasValue)
                .WithMessage("Cost price cannot be negative.");

            RuleFor(x => x.Sku)
                .MaximumLength(100)
                .When(x => x.Sku is not null);

            RuleFor(x => x.Barcode)
                .MaximumLength(100)
                .When(x => x.Barcode is not null);

            RuleFor(x => x.WeightKg)
                .GreaterThan(0)
                .When(x => x.WeightKg.HasValue)
                .WithMessage("Weight must be greater than zero.");

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl))
                .WithMessage("Image URL must be a valid URL.");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0);

            // Validate each attribute
            RuleForEach(x => x.Attributes)
                .SetValidator(new ProductAttributeDtoValidator());

            // Attribute names must be unique within same variant
            RuleFor(x => x.Attributes)
                .Must(attrs =>
                    attrs.Select(a => a.Name.ToLower()).Distinct().Count()
                    == attrs.Count)
                .When(x => x.Attributes.Count > 0)
                .WithMessage("Duplicate attribute names are not allowed in the same variant.");
        }
    }


}
