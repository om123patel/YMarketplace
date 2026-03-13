using Catalog.Application.DTOs;
using FluentValidation;

namespace Catalog.Application.Validators.AttributeTemplate
{
    public class UpdateAttributeTemplateDtoValidator
    : AbstractValidator<UpdateAttributeTemplateDto>
    {
        public UpdateAttributeTemplateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Template name is required.")
                .MaximumLength(100).WithMessage("Template name cannot exceed 100 characters.");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("At least one attribute item is required.");

            RuleForEach(x => x.Items)
                .SetValidator(new CreateAttributeTemplateItemDtoValidator());

            RuleFor(x => x.Items)
                .Must(items =>
                    items.Select(i => i.AttributeName.ToLower()).Distinct().Count()
                    == items.Count)
                .WithMessage("Duplicate attribute names are not allowed in the same template.");
        }
    }

}
