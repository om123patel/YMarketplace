using Catalog.Application.DTOs.Products;
using Catalog.Application.Validators.Product;
using FluentValidation;

namespace Catalog.Application.Validators
{
    public class CreateSellerProductDtoValidator
    : AbstractValidator<CreateSellerProductDto>
    {
        public CreateSellerProductDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(300);

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Slug is required.")
                .MaximumLength(350)
                .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
                .WithMessage("Slug must be lowercase letters, numbers and hyphens only.");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("A valid category is required.");

            RuleFor(x => x.BasePrice)
                .GreaterThan(0).WithMessage("Base price must be greater than zero.");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty().Length(3)
                .WithMessage("Currency code must be exactly 3 characters.");

            RuleFor(x => x.CompareAtPrice)
                .GreaterThanOrEqualTo(x => x.BasePrice)
                .When(x => x.CompareAtPrice.HasValue)
                .WithMessage("Compare at price must be >= base price.");

            RuleFor(x => x.ShortDescription)
                .MaximumLength(500)
                .When(x => x.ShortDescription is not null);

            RuleFor(x => x.Sku)
                .MaximumLength(100)
                .When(x => x.Sku is not null);

            RuleFor(x => x.WeightKg)
                .GreaterThan(0)
                .When(x => x.WeightKg.HasValue);

            RuleFor(x => x.ImageUrls)
                .Must(urls => urls.Count <= 10)
                .WithMessage("Maximum 10 images allowed.");

            RuleFor(x => x.TagIds)
                .Must(tags => tags.Count <= 20)
                .WithMessage("Maximum 20 tags allowed.");

            RuleForEach(x => x.Variants)
                .SetValidator(new CreateProductVariantDtoValidator());
        }
    }

}
