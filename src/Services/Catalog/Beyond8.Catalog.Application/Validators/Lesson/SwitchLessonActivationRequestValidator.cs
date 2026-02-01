using Beyond8.Catalog.Application.Dtos.Lessons;
using FluentValidation;

namespace Beyond8.Catalog.Application.Validators.Lesson;

public class SwitchLessonActivationRequestValidator : AbstractValidator<SwitchLessonActivationRequest>
{
    public SwitchLessonActivationRequestValidator()
    {
        RuleFor(x => x.IsPublished)
            .NotNull()
            .WithMessage("IsPublished is required.");
    }
}