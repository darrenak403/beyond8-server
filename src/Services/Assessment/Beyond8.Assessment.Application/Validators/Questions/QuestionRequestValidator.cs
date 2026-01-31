using Beyond8.Assessment.Application.Dtos.Questions;
using FluentValidation;

namespace Beyond8.Assessment.Application.Validators.Questions
{
    public class QuestionRequestValidator : AbstractValidator<QuestionRequest>
    {
        public QuestionRequestValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content không được để trống")
                .MaximumLength(1000).WithMessage("Content không được vượt quá 1000 ký tự");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Type không hợp lệ");

            RuleFor(x => x.Options)
                .NotEmpty().WithMessage("Options không được để trống")
                .ForEach(x => x.SetValidator(new QuestionOptionItemValidator()));

            RuleFor(x => x.Explanation)
                .MaximumLength(1000).WithMessage("Explanation không được vượt quá 1000 ký tự");

            RuleFor(x => x.Tags)
                .NotEmpty().WithMessage("Tags không được để trống")
                .ForEach(x => x.MaximumLength(100).WithMessage("Tag không được vượt quá 100 ký tự"));

            RuleFor(x => x.Difficulty)
                .IsInEnum().WithMessage("Difficulty không hợp lệ");

            RuleFor(x => x.Points)
                .GreaterThanOrEqualTo(0).WithMessage("Points phải lớn hơn hoặc bằng 0");
        }
    }
}