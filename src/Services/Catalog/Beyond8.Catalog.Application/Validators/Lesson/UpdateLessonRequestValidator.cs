using Beyond8.Catalog.Application.Dtos.Lessons;
using FluentValidation;

namespace Beyond8.Catalog.Application.Validators.Lesson;

public class UpdateLessonRequestValidator : AbstractValidator<UpdateLessonRequest>
{
    public UpdateLessonRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề bài học không được để trống")
            .MaximumLength(200).WithMessage("Tiêu đề bài học không được vượt quá 200 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả bài học không được vượt quá 1000 ký tự");

        RuleFor(x => x.MinCompletionSeconds)
            .GreaterThanOrEqualTo(0).WithMessage("Thời gian hoàn thành tối thiểu phải lớn hơn hoặc bằng 0");

        RuleFor(x => x.RequiredScore)
            .InclusiveBetween(0, 100).WithMessage("Điểm yêu cầu phải từ 0 đến 100");
    }
}