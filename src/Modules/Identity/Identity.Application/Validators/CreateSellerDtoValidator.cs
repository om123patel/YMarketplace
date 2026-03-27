using FluentValidation;
using Identity.Application.DTOs.Seller;

namespace Identity.Application.Validators
{
    public class CreateSellerDtoValidator : AbstractValidator<CreateSellerDto>
    {
        public CreateSellerDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.");

            RuleFor(x => x.BusinessName)
                .NotEmpty().WithMessage("Business name is required.")
                .MaximumLength(200);

            RuleFor(x => x.BusinessEmail)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.BusinessEmail))
                .MaximumLength(255);

            RuleFor(x => x.BusinessPhone)
                .MaximumLength(20)
                .Matches(@"^[0-9+\-\s()]*$")
                .When(x => !string.IsNullOrEmpty(x.BusinessPhone))
                .WithMessage("Invalid phone number format.");

            RuleFor(x => x.Description)
                .MaximumLength(1000);

            RuleFor(x => x.WebsiteUrl)
                .Must(BeAValidUrl)
                .When(x => !string.IsNullOrEmpty(x.WebsiteUrl))
                .WithMessage("Invalid website URL.")
                .MaximumLength(500);

            RuleFor(x => x.Address)
                .NotNull().WithMessage("Address is required.");

            When(x => x.Address != null, () =>
            {
                RuleFor(x => x.Address.AddressLine1)
                    .MaximumLength(200);

                RuleFor(x => x.Address.AddressLine2)
                    .MaximumLength(200);

                RuleFor(x => x.Address.City)
                    .MaximumLength(100);

                RuleFor(x => x.Address.State)
                    .MaximumLength(100);

                RuleFor(x => x.Address.PostalCode)
                    .MaximumLength(20);

                RuleFor(x => x.Address.Country)
                    .MaximumLength(100);
            });
        }

        private bool BeAValidUrl(string? url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}
