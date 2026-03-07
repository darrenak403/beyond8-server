using Beyond8.Learning.Application.Dtos.Certificates;
using FluentValidation;

namespace Beyond8.Learning.Application.Validators.Certificates;

public class UpdateCertificateEligibilityConfigRequestValidator : AbstractValidator<UpdateCertificateEligibilityConfigRequest>
{
    public UpdateCertificateEligibilityConfigRequestValidator()
    {
        RuleFor(x => x.QuizAverageMinPercent)
            .InclusiveBetween(0, 100).WithMessage("Điểm quiz tối thiểu phải từ 0 đến 100")
            .When(x => x.QuizAverageMinPercent.HasValue);

        RuleFor(x => x.AssignmentAverageMinPercent)
            .InclusiveBetween(0, 100).WithMessage("Điểm assignment tối thiểu phải từ 0 đến 100")
            .When(x => x.AssignmentAverageMinPercent.HasValue);
    }
}
