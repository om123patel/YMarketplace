using FluentValidation;
using Orders.Application.DTOs.Dispute;

namespace Orders.Application.Validators
{
    public class OpenDisputeDtoValidator : AbstractValidator<OpenDisputeDto>
    {
        public OpenDisputeDtoValidator()
        {
            RuleFor(x => x.Reason).NotEmpty()
                .MinimumLength(10).WithMessage("Please provide a meaningful dispute reason (min 10 characters).")
                .MaximumLength(1000);
            RuleFor(x => x.Evidence).MaximumLength(2000)
                .When(x => x.Evidence is not null);
        }
    }

}
