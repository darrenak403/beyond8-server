using Beyond8.Assessment.Application.Dtos.Assignments;
using FluentValidation;

namespace Beyond8.Assessment.Application.Validators.Assignments;

public class CreateAssignmentRequestValidator : AbstractValidator<CreateAssignmentRequest>
{
    public CreateAssignmentRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề không được để trống")
            .MaximumLength(200).WithMessage("Tiêu đề không được vượt quá 200 ký tự");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Mô tả không được để trống");

        RuleFor(x => x.MaxTextLength)
            .GreaterThan(0).WithMessage("Độ dài tối đa nội dung phải lớn hơn 0");

        RuleFor(x => x.TotalPoints)
            .GreaterThanOrEqualTo(0).WithMessage("Tổng điểm phải lớn hơn hoặc bằng 0");

        RuleFor(x => x.TimeLimitMinutes)
            .GreaterThan(0).WithMessage("Thời gian làm bài (phút) phải lớn hơn 0")
            .When(x => x.TimeLimitMinutes.HasValue);

        RuleFor(x => x.SubmissionType)
            .IsInEnum().WithMessage("Loại nộp bài không hợp lệ");

        RuleFor(x => x.GradingMode)
            .IsInEnum().WithMessage("Chế độ chấm điểm không hợp lệ");
    }
}
