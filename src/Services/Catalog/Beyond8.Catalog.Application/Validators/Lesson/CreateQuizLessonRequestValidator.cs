using Beyond8.Catalog.Application.Dtos.Lessons;
using FluentValidation;

namespace Beyond8.Catalog.Application.Validators.Lesson;

public class CreateQuizLessonRequestValidator : AbstractValidator<CreateQuizLessonRequest>
{
    public CreateQuizLessonRequestValidator()
    {
        RuleFor(x => x.SectionId)
            .NotEmpty().WithMessage("SectionId không được để trống");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề bài học không được để trống")
            .MaximumLength(200).WithMessage("Tiêu đề bài học không được vượt quá 200 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả bài học không được vượt quá 1000 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Description));

        // Quiz-specific validation
        RuleFor(x => x.QuizId)
            .NotEmpty().WithMessage("QuizId không được để trống cho bài học quiz");
    }
}