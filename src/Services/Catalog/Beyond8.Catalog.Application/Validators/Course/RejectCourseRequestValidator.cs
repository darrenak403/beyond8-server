using FluentValidation;
using Beyond8.Catalog.Application.Dtos.Courses;

namespace Beyond8.Catalog.Application.Validators.Course;

public class RejectCourseRequestValidator : AbstractValidator<RejectCourseRequest>
{
    public RejectCourseRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Lý do từ chối không được để trống")
            .MinimumLength(20).WithMessage("Lý do từ chối phải có ít nhất 20 ký tự")
            .MaximumLength(1000).WithMessage("Lý do từ chối không được vượt quá 1000 ký tự");
    }
}