using Beyond8.Integration.Application.Dtos.AiIntegration.Quiz;
using FluentValidation;

namespace Beyond8.Integration.Application.Validators.AiIntegration
{
    public class GenQuizRequestValidator : AbstractValidator<GenQuizRequest>
    {
        public GenQuizRequestValidator()
        {
            RuleFor(x => x.CourseId)
                .NotEmpty().WithMessage("CourseId không được để trống");

            RuleFor(x => x.TotalCount)
                .InclusiveBetween(5, 50).WithMessage("Tổng số câu hỏi phải từ 5 đến 50");

            RuleFor(x => x.MaxPoints)
                .InclusiveBetween(100, 1000).WithMessage("Điểm tối đa phải từ 100 đến 1000");

            RuleFor(x => x.Distribution)
                .Must(d => d == null || (d.EasyPercent + d.MediumPercent + d.HardPercent == 100))
                .WithMessage("Tổng tỷ lệ độ khó phải bằng 100%")
                .When(x => x.Distribution != null);

            RuleFor(x => x.Distribution!.EasyPercent)
                .InclusiveBetween(0, 100).WithMessage("Tỷ lệ Easy phải từ 0 đến 100")
                .When(x => x.Distribution != null);

            RuleFor(x => x.Distribution!.MediumPercent)
                .InclusiveBetween(0, 100).WithMessage("Tỷ lệ Medium phải từ 0 đến 100")
                .When(x => x.Distribution != null);

            RuleFor(x => x.Distribution!.HardPercent)
                .InclusiveBetween(0, 100).WithMessage("Tỷ lệ Hard phải từ 0 đến 100")
                .When(x => x.Distribution != null);

            RuleFor(x => x.TopK)
                .InclusiveBetween(1, 50).WithMessage("TopK phải từ 1 đến 50")
                .When(x => x.TopK != 0);
        }
    }
}
