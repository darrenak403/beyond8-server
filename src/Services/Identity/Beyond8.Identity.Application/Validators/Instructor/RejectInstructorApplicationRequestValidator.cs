using Beyond8.Identity.Application.Dtos.Instructors;
using FluentValidation;

namespace Beyond8.Identity.Application.Validators.Instructor;

public class RejectInstructorApplicationRequestValidator : AbstractValidator<RejectInstructorApplicationRequest>
{
    public RejectInstructorApplicationRequestValidator()
    {
        RuleFor(x => x.VerificationNotes)
            .NotEmpty().WithMessage("Lý do từ chối không được để trống")
            .MinimumLength(10).WithMessage("Lý do từ chối phải có ít nhất 10 ký tự")
            .MaximumLength(500).WithMessage("Lý do từ chối không được vượt quá 500 ký tự")
            .Matches(@"^[^\s].*[^\s]$").WithMessage("Lý do từ chối không được bắt đầu hoặc kết thúc bằng khoảng trắng");
    }
}