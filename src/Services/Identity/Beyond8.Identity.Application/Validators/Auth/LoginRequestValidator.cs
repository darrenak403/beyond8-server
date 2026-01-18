using Beyond8.Identity.Application.Dtos.Auth;
using FluentValidation;

namespace Beyond8.Identity.Application.Validators.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email không được để trống")
            .EmailAddress()
            .WithMessage("Email không hợp lệ")
            .MaximumLength(256)
            .WithMessage("Email không được vượt quá 256 ký tự");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password không được để trống")
            .MinimumLength(8)
            .WithMessage("Password tối thiểu 8 ký tự")
            .MaximumLength(100)
            .WithMessage("Password không được vượt quá 100 ký tự");
    }
}
