using Beyond8.Assessment.Application.Dtos.Quizzes;
using FluentValidation;

namespace Beyond8.Assessment.Application.Validators.Quizzes
{
    public class CreateQuizRequestValidator : AbstractValidator<CreateQuizRequest>
    {
        public CreateQuizRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title không được để trống")
                .MaximumLength(200).WithMessage("Title không được vượt quá 200 ký tự");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description không được vượt quá 1000 ký tự");
        }
    }
}