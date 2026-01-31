using Beyond8.Catalog.Application.Dtos.Lessons;
using FluentValidation;

namespace Beyond8.Catalog.Application.Validators.Lessons;

public class ReorderLessonInSectionRequestValidator : AbstractValidator<ReorderLessonInSectionRequest>
{
    public ReorderLessonInSectionRequestValidator()
    {
        RuleFor(x => x.LessonId)
            .NotEmpty().WithMessage("LessonId không được để trống");

        RuleFor(x => x.NewOrderIndex)
            .GreaterThan(0).WithMessage("NewOrderIndex phải lớn hơn 0");
    }
}