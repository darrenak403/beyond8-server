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
                .MaximumLength(500).WithMessage("Text không được vượt quá 500 ký tự");

            RuleFor(x => x.IsCorrect)
                .Must(x => x == true || x == false).WithMessage("IsCorrect không hợp lệ");
        }
    }
}