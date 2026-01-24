using Beyond8.Identity.Application.Dtos.Instructors;
using Beyond8.Identity.Domain.Enums;
using FluentValidation;

namespace Beyond8.Identity.Application.Validators.Instructor;

public class NotApproveInstructorProfileRequestValidator : AbstractValidator<NotApproveInstructorProfileRequest>
{
    public NotApproveInstructorProfileRequestValidator()
    {
        RuleFor(x => x.NotApproveReason)
            .NotEmpty().WithMessage("Lý do không phê duyệt không được để trống")
            .MinimumLength(10).WithMessage("Lý do không phê duyệt phải có ít nhất 10 ký tự")
            .MaximumLength(500).WithMessage("Lý do không phê duyệt không được vượt quá 500 ký tự")
            .Matches(@"^[^\s].*[^\s]$").WithMessage("Lý do không phê duyệt không được bắt đầu hoặc kết thúc bằng khoảng trắng");

        RuleFor(x => x.VerificationStatus)
            .IsInEnum().WithMessage("Trạng thái không hợp lệ")
            .Must(status => status == VerificationStatus.RequestUpdate)
            .WithMessage("Trạng thái phải là RequestUpdate");
    }
}