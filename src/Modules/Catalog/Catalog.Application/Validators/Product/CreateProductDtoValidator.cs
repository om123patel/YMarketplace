using Catalog.Application.DTOs;
using FluentValidation;

namespace Catalog.Application.Validators.Product
{
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator()
        {
            // Core
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(300).WithMessage("Product name cannot exceed 300 characters.");

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Slug is required.")
                .MaximumLength(350).WithMessage("Slug cannot exceed 350 characters.")
                .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
                .WithMessage("Slug must be lowercase letters, numbers and hyphens only.");

            RuleFor(x => x.ShortDescription)
                .MaximumLength(500)
                .When(x => x.ShortDescription is not null)
                .WithMessage("Short description cannot exceed 500 characters.");

            // Classification
            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("A valid category is required.");

            RuleFor(x => x.BrandId)
                .GreaterThan(0)
                .When(x => x.BrandId.HasValue)
                .WithMessage("Brand ID must be greater than zero.");

            // Pricing
            RuleFor(x => x.BasePrice)
                .GreaterThan(0).WithMessage("Base price must be greater than zero.");

            RuleFor(x => x.CurrencyCode)
                .NotEmpty().WithMessage("Currency code is required.")
                .Length(3).WithMessage("Currency code must be exactly 3 characters (e.g. USD).");

            RuleFor(x => x.CompareAtPrice)
                .GreaterThanOrEqualTo(x => x.BasePrice)
                .When(x => x.CompareAtPrice.HasValue)
                .WithMessage("Compare at price must be greater than or equal to base price.");

            RuleFor(x => x.CostPrice)
                .GreaterThanOrEqualTo(0)
                .When(x => x.CostPrice.HasValue)
                .WithMessage("Cost price cannot be negative.");

            // Identifiers
            RuleFor(x => x.Sku)
                .MaximumLength(100)
                .When(x => x.Sku is not null);

            RuleFor(x => x.Barcode)
                .MaximumLength(100)
                .When(x => x.Barcode is not null);

            // Physical
            RuleFor(x => x.WeightKg)
                .GreaterThan(0)
                .When(x => x.WeightKg.HasValue)
                .WithMessage("Weight must be greater than zero.");

            RuleFor(x => x.LengthCm)
                .GreaterThan(0)
                .When(x => x.LengthCm.HasValue)
                .WithMessage("Length must be greater than zero.");

            RuleFor(x => x.WidthCm)
                .GreaterThan(0)
                .When(x => x.WidthCm.HasValue)
                .WithMessage("Width must be greater than zero.");

            RuleFor(x => x.HeightCm)
                .GreaterThan(0)
                .When(x => x.HeightCm.HasValue)
                .WithMessage("Height must be greater than zero.");

            // SEO
            RuleFor(x => x.Seo!.MetaTitle)
                .MaximumLength(70)
                .When(x => x.Seo?.MetaTitle is not null)
                .WithMessage("Meta title should not exceed 70 characters.");

            RuleFor(x => x.Seo!.MetaDescription)
                .MaximumLength(165)
                .When(x => x.Seo?.MetaDescription is not null)
                .WithMessage("Meta description should not exceed 165 characters.");

            RuleFor(x => x.Seo!.OgImageUrl)
                .MaximumLength(500)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrWhiteSpace(x.Seo?.OgImageUrl))
                .WithMessage("OG Image URL must be a valid URL.");

            // Images
            RuleForEach(x => x.ImageUrls)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Each image URL must be a valid URL.");

            RuleFor(x => x.ImageUrls)
                .Must(urls => urls.Count <= 10)
                .WithMessage("A product cannot have more than 10 images.");

            // Tags
            RuleFor(x => x.TagIds)
                .Must(tags => tags.Count <= 20)
                .WithMessage("A product cannot have more than 20 tags.");

            RuleForEach(x => x.TagIds)
                .GreaterThan(0)
                .WithMessage("Each tag ID must be greater than zero.");

            // Variants
            RuleFor(x => x.Variants)
                .Must(v => v.Count <= 100)
                .WithMessage("A product cannot have more than 100 variants.");

            RuleForEach(x => x.Variants)
                .SetValidator(new CreateProductVariantDtoValidator());

            // SKUs must be unique across variants within same product
            RuleFor(x => x.Variants)
                .Must(variants =>
                {
                    var skus = variants
                        .Where(v => !string.IsNullOrWhiteSpace(v.Sku))
                        .Select(v => v.Sku!.ToLower())
                        .ToList();
                    return skus.Distinct().Count() == skus.Count;
                })
                .When(x => x.Variants.Count > 0)
                .WithMessage("Duplicate SKUs are not allowed across variants.");
        }
    }


}
