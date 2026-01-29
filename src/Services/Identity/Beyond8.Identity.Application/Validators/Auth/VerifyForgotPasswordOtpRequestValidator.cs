using Beyond8.Identity.Application.Dtos.Auth;
using FluentValidation;

namespace Beyond8.Identity.Application.Validators.Auth;

public class VerifyForgotPasswordOtpRequestValidator : AbstractValidator<VerifyForgotPasswordOtpRequest>
{
    public VerifyForgotPasswordOtpRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email không được để trống")
            .EmailAddress()
            .WithMessage("Email không hợp lệ")
            .MaximumLength(256)
            .WithMessage("Email không được vượt quá 256 ký tự");

        RuleFor(x => x.OtpCode)
            .NotEmpty()
            .WithMessage("Mã OTP không được để trống")
            .Length(6)
            .WithMessage("Mã OTP phải bao gồm 6 ký tự")
            .Matches(@"^\d+$")
            .WithMessage("Mã OTP chỉ được chứa số");
    }
}
