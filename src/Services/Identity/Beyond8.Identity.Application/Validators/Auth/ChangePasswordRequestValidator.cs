using Beyond8.Identity.Application.Dtos.Auth;
using FluentValidation;

namespace Beyond8.Identity.Application.Validators.Auth
{
    public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(x => x.OldPassword)
                .NotEmpty()
                .WithMessage("Mật khẩu cũ không được để trống")
                .MinimumLength(8)
                .WithMessage("Mật khẩu cũ tối thiểu 8 ký tự")
                .MaximumLength(100)
                .WithMessage("Mật khẩu cũ không được vượt quá 100 ký tự");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("Mật khẩu mới không được để trống")
                .MinimumLength(8)
                .WithMessage("Mật khẩu mới tối thiểu 8 ký tự")
                .MaximumLength(100)
                .WithMessage("Mật khẩu mới không được vượt quá 100 ký tự")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")
                .WithMessage("Mật khẩu mới phải có ít nhất 1 chữ thường, 1 chữ hoa và 1 số")
                .NotEqual(x => x.OldPassword)
                .WithMessage("Mật khẩu mới phải khác mật khẩu cũ");
        }
    }
}
