using Beyond8.Identity.Application.Dtos.Auth;
using FluentValidation;

namespace Beyond8.Identity.Application.Validators.Auth
{
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
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
                .WithMessage("Mã OTP phải gồm 6 chữ số")
                .Matches(@"^\d+$")
                .WithMessage("Mã OTP chỉ được chứa số");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("Mật khẩu mới không được để trống")
                .MinimumLength(8)
                .WithMessage("Mật khẩu tối thiểu 8 ký tự")
                .MaximumLength(100)
                .WithMessage("Mật khẩu không được vượt quá 100 ký tự")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")
                .WithMessage("Mật khẩu phải có ít nhất 1 chữ thường, 1 chữ hoa và 1 số");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty()
                .WithMessage("Xác nhận mật khẩu không được để trống")
                .Equal(x => x.NewPassword)
                .WithMessage("Mật khẩu xác nhận không khớp");
        }
    }
}
