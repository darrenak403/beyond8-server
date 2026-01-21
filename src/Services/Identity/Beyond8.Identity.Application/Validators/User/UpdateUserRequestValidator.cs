using Beyond8.Identity.Application.Dtos.Users;
using FluentValidation;

namespace Beyond8.Identity.Application.Validators.Users;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {

        When(x => !string.IsNullOrEmpty(x.FullName), () =>
        {
            RuleFor(x => x.FullName)
                .MaximumLength(100).WithMessage("Họ tên không được vượt quá 100 ký tự");
        });

        When(x => x.DateOfBirth.HasValue, () =>
        {
            RuleFor(x => x.DateOfBirth!.Value.Date)
                .LessThanOrEqualTo(DateTime.Now.Date)
                .WithMessage("Ngày sinh không được là ngày trong tương lai")
                .GreaterThan(DateTime.Now.AddYears(-120).Date)
                .WithMessage("Ngày sinh không hợp lệ");
        });

        When(x => !string.IsNullOrEmpty(x.PhoneNumber), () =>
        {
            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("Số điện thoại không được vượt quá 20 ký tự")
                .Matches(@"^\+?[0-9\s\-\(\)]+$").WithMessage("Số điện thoại không hợp lệ");
        });

        When(x => !string.IsNullOrEmpty(x.Timezone), () =>
        {
            RuleFor(x => x.Timezone)
                .MaximumLength(50).WithMessage("Timezone không được vượt quá 50 ký tự");
        });

        When(x => !string.IsNullOrEmpty(x.Locale), () =>
        {
            RuleFor(x => x.Locale)
                .MaximumLength(10).WithMessage("Locale không được vượt quá 10 ký tự")
                .Matches(@"^[a-z]{2}(-[A-Z]{2})?$").WithMessage("Locale không hợp lệ (ví dụ: vi, vi-VN, en-US)");
        });
    }
}