// src/Modules/Identity/Identity.Application/Validators/CustomerLoginDtoValidator.cs
using FluentValidation;
using Identity.Application.DTOs.Customer;

namespace Identity.Application.Validators
{
    public class CustomerLoginDtoValidator : AbstractValidator<CustomerLoginDto>
    {
        public CustomerLoginDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}