using Beyond8.Identity.Application.Dtos.Auth;
using FluentValidation;

namespace Beyond8.Identity.Application.Validators.Auth;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token không được để trống")
            .MinimumLength(20)
            .WithMessage("Refresh token không hợp lệ")
            .MaximumLength(500)
            .WithMessage("Refresh token không hợp lệ");
    }
}
