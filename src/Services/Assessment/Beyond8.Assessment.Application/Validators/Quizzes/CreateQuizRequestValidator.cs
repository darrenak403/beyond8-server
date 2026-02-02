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

            RuleFor(x => x.TimeLimitMinutes)
                .GreaterThanOrEqualTo(0).WithMessage("TimeLimitMinutes phải lớn hơn hoặc bằng 0")
                .When(x => x.TimeLimitMinutes.HasValue);

            RuleFor(x => x.PassScorePercent)
                .InclusiveBetween(0, 100).WithMessage("PassScorePercent phải từ 0 đến 100");

            RuleFor(x => x.TotalPoints)
                .GreaterThanOrEqualTo(0).WithMessage("TotalPoints phải lớn hơn hoặc bằng 0");

            RuleFor(x => x.MaxAttempts)
                .GreaterThanOrEqualTo(1).WithMessage("MaxAttempts phải lớn hơn hoặc bằng 1");

            RuleFor(x => x.ShuffleQuestions)
                .Must(x => x == true || x == false).WithMessage("ShuffleQuestions phải là true hoặc false");

            RuleFor(x => x.AllowReview)
                .Must(x => x == true || x == false).WithMessage("AllowReview phải là true hoặc false");

            RuleFor(x => x.ShowExplanation)
                .Must(x => x == true || x == false).WithMessage("ShowExplanation phải là true hoặc false");

            RuleFor(x => x.QuestionIds)
                .NotEmpty().WithMessage("QuestionIds không được để trống")
                .Must(x => x.Count > 0).WithMessage("QuestionIds phải có ít nhất 1 câu hỏi");

            RuleFor(x => x.DifficultyDistribution)
                .Must(d => d == null || (d.EasyPercent >= 0 && d.EasyPercent <= 100
                    && d.MediumPercent >= 0 && d.MediumPercent <= 100
                    && d.HardPercent >= 0 && d.HardPercent <= 100
                    && d.EasyPercent + d.MediumPercent + d.HardPercent == 100))
                .WithMessage("DifficultyDistribution: EasyPercent, MediumPercent, HardPercent phải từ 0-100 và tổng bằng 100")
                .When(x => x.DifficultyDistribution != null);
        }
    }
}