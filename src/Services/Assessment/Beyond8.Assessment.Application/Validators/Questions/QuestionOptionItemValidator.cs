using Beyond8.Assessment.Domain.JSONFields;
using FluentValidation;

namespace Beyond8.Assessment.Application.Validators.Questions
{
    public class QuestionOptionItemValidator : AbstractValidator<QuestionOptionItem>
    {
        public QuestionOptionItemValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id không được để trống");

            RuleFor(x => x.Text)
                .NotEmpty().WithMessage("Text không được để trống")
                .MaximumLength(100).WithMessage("Text không được vượt quá 100 ký tự");

            RuleFor(x => x.IsCorrect)
                .IsInEnum().WithMessage("IsCorrect không hợp lệ");
        }
    }
}