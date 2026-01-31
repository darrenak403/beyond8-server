using FluentValidation;
using Beyond8.Catalog.Application.Dtos.Lessons;

namespace Beyond8.Catalog.Application.Validators.Lesson;

public class ChangeQuizForLessonValidator : AbstractValidator<ChangeQuizForLessonRequest>
{
    public ChangeQuizForLessonValidator()
    {
        RuleFor(x => x.QuizId)
            .NotEmpty().WithMessage("QuizId không được để trống");
    }
}