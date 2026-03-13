using Catalog.Application.DTOs;
using FluentValidation;

namespace Catalog.Application.Validators.AttributeTemplate
{
    public class CreateAttributeTemplateItemDtoValidator
     : AbstractValidator<CreateAttributeTemplateItemDto>
    {
        private static readonly string[] AllowedInputTypes =
            ["Text", "Select", "MultiSelect", "Number", "Boolean"];

        public CreateAttributeTemplateItemDtoValidator()
        {
            RuleFor(x => x.AttributeName)
                .NotEmpty().WithMessage("Attribute name is required.")
                .MaximumLength(100).WithMessage("Attribute name cannot exceed 100 characters.");

            RuleFor(x => x.InputType)
                .NotEmpty().WithMessage("Input type is required.")
                .Must(t => AllowedInputTypes.Contains(t))
                .WithMessage($"Input type must be one of: {string.Join(", ", AllowedInputTypes)}.");

            // Options are required when InputType is Select or MultiSelect
            RuleFor(x => x.Options)
                .NotEmpty()
                .When(x => x.InputType is "Select" or "MultiSelect")
                .WithMessage("Options are required for Select and MultiSelect input types.");

            RuleFor(x => x.Options)
                .Must(opts => opts.All(o => !string.IsNullOrWhiteSpace(o)))
                .When(x => x.Options.Count > 0)
                .WithMessage("Options cannot contain empty values.");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Sort order cannot be negative.");
        }
    }

}
