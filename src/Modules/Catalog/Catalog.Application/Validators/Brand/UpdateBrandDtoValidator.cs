using Catalog.Application.DTOs;
using FluentValidation;

namespace Catalog.Application.Validators.Brand
{
    public class UpdateBrandDtoValidator : AbstractValidator<UpdateBrandDto>
    {
        public UpdateBrandDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Brand name is required.")
                .MaximumLength(100).WithMessage("Brand name cannot exceed 100 characters.");

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Slug is required.")
                .MaximumLength(120).WithMessage("Slug cannot exceed 120 characters.")
                .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
                .WithMessage("Slug must be lowercase letters, numbers and hyphens only.");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => x.Description is not null);

            RuleFor(x => x.LogoUrl)
                .MaximumLength(500)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrWhiteSpace(x.LogoUrl))
                .WithMessage("Logo URL must be a valid URL.");

            RuleFor(x => x.WebsiteUrl)
                .MaximumLength(300)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrWhiteSpace(x.WebsiteUrl))
                .WithMessage("Website URL must be a valid URL.");
        }
    }

}
