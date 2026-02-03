using Beyond8.Assessment.Application.Dtos.AssignmentSubmissions;
using FluentValidation;

namespace Beyond8.Assessment.Application.Validators.AssignmentSubmissions;

public class GradeSubmissionRequestValidator : AbstractValidator<GradeSubmissionRequest>
{
    public GradeSubmissionRequestValidator()
    {
        RuleFor(x => x.FinalScore)
            .InclusiveBetween(0, 100).WithMessage("Điểm số phải từ 0 đến 100");

        RuleFor(x => x.InstructorFeedback)
            .MaximumLength(2000).WithMessage("Nhận xét không được quá 2000 ký tự")
            .When(x => !string.IsNullOrEmpty(x.InstructorFeedback));
    }
}
