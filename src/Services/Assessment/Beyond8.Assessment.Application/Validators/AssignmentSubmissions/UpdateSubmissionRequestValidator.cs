using Beyond8.Assessment.Application.Dtos.AssignmentSubmissions;
using FluentValidation;

namespace Beyond8.Assessment.Application.Validators.AssignmentSubmissions;

public class UpdateSubmissionRequestValidator : AbstractValidator<UpdateSubmissionRequest>
{
    public UpdateSubmissionRequestValidator()
    {
        RuleFor(x => x.TextContent)
            .MaximumLength(10000).WithMessage("Nội dung không được quá 10000 ký tự")
            .When(x => !string.IsNullOrEmpty(x.TextContent));
    }
}
