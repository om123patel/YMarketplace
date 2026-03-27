using FluentValidation;
using Identity.Application.DTOs.Seller;

namespace Identity.Application.Validators
{
    public class UpdateSellerDtoValidator : AbstractValidator<UpdateSellerDto>
    {
        public UpdateSellerDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.BusinessName)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.BusinessEmail)
                .EmailAddress()
                .When(x => !string.IsNullOrEmpty(x.BusinessEmail))
                .MaximumLength(255);

            RuleFor(x => x.BusinessPhone)
                .MaximumLength(20)
                .Matches(@"^[0-9+\-\s()]*$")
                .When(x => !string.IsNullOrEmpty(x.BusinessPhone));

            RuleFor(x => x.Description)
                .MaximumLength(1000);

            RuleFor(x => x.WebsiteUrl)
                .Must(BeAValidUrl)
                .When(x => !string.IsNullOrEmpty(x.WebsiteUrl))
                .MaximumLength(500);
        }

        private bool BeAValidUrl(string? url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}
