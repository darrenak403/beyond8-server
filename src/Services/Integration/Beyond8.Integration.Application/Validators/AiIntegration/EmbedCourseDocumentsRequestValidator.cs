using Beyond8.Integration.Application.Dtos.AiIntegration.Embedding;
using FluentValidation;

namespace Beyond8.Integration.Application.Validators.AiIntegration;

public class EmbedCourseDocumentsRequestValidator : AbstractValidator<EmbedCourseDocumentsRequest>
{
    public EmbedCourseDocumentsRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("CourseId không được để trống");

        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("DocumentId không được để trống");
    }
}
