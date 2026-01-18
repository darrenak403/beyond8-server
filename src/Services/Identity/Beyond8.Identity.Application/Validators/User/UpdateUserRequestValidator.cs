using Beyond8.Identity.Application.Dtos.Users;
using FluentValidation;

namespace Beyond8.Identity.Application.Validators.Users;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        When(x => !string.IsNullOrEmpty(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email không hợp lệ")
                .MaximumLength(255).WithMessage("Email không được vượt quá 255 ký tự");
        });

        When(x => !string.IsNullOrEmpty(x.FullName), () =>
        {
            RuleFor(x => x.FullName)
                .MaximumLength(100).WithMessage("Họ tên không được vượt quá 100 ký tự");
        });

        When(x => !string.IsNullOrEmpty(x.PhoneNumber), () =>
        {
            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("Số điện thoại không được vượt quá 20 ký tự")
                .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("Số điện thoại không hợp lệ");
        });
    }
}