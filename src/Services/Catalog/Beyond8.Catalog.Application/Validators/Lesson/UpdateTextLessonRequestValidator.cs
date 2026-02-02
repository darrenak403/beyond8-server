using Beyond8.Catalog.Application.Dtos.Lessons;
using FluentValidation;

namespace Beyond8.Catalog.Application.Validators.Lesson;

public class UpdateTextLessonRequestValidator : AbstractValidator<UpdateTextLessonRequest>
{
    public UpdateTextLessonRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id bài học không được để trống");

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id bài học không được để trống");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề bài học không được để trống")
            .MaximumLength(200).WithMessage("Tiêu đề bài học không được vượt quá 200 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả bài học không được vượt quá 1000 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Description));

        // Text-specific validation
        RuleFor(x => x.Content)
            .MaximumLength(50000).WithMessage("Nội dung bài học không được vượt quá 50000 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Content));
    }
}