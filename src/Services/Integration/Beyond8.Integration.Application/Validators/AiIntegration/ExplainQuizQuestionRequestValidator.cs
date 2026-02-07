using Beyond8.Integration.Application.Dtos.AiIntegration.Quiz;
using FluentValidation;

namespace Beyond8.Integration.Application.Validators.AiIntegration
{
    public class ExplainQuizQuestionRequestValidator : AbstractValidator<ExplainQuizQuestionRequest>
    {
        public ExplainQuizQuestionRequestValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Nội dung câu hỏi không được để trống");

            RuleForEach(x => x.Options)
                .ChildRules(opt =>
                {
                    opt.RuleFor(o => o.Text)
                        .NotEmpty().WithMessage("Nội dung lựa chọn không được để trống");
                })
                .When(x => x.Options != null && x.Options.Count > 0);
        }
    }
}
