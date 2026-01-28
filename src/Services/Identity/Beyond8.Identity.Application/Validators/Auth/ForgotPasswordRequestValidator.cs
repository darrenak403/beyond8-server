using Beyond8.Identity.Application.Dtos.Auth;
using FluentValidation;

namespace Beyond8.Identity.Application.Validators.Auth
{
    public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email không được để trống")
                .EmailAddress()
                .WithMessage("Email không hợp lệ")
                .MaximumLength(256)
                .WithMessage("Email không được vượt quá 256 ký tự");
        }
    }
}
