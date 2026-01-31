using Beyond8.Assessment.Application.Dtos.Questions;
using FluentValidation;

namespace Beyond8.Assessment.Application.Validators.Questions;

public class QuestionFromAiRequestValidator : AbstractValidator<QuestionFromAiRequest>
{
    public QuestionFromAiRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => (x.Easy?.Count ?? 0) + (x.Medium?.Count ?? 0) + (x.Hard?.Count ?? 0) > 0)
            .WithMessage("Phải có ít nhất một câu hỏi trong Easy, Medium hoặc Hard.");

        RuleFor(x => x.Easy)
            .NotNull().WithMessage("Easy không được null");

        RuleFor(x => x.Medium)
            .NotNull().WithMessage("Medium không được null");

        RuleFor(x => x.Hard)
            .NotNull().WithMessage("Hard không được null");

        RuleForEach(x => x.Easy).SetValidator(new QuestionRequestValidator());
        RuleForEach(x => x.Medium).SetValidator(new QuestionRequestValidator());
        RuleForEach(x => x.Hard).SetValidator(new QuestionRequestValidator());
    }
}
