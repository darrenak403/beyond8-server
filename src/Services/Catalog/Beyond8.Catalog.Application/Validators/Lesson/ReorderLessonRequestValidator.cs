using Beyond8.Catalog.Application.Dtos.Lessons;
using FluentValidation;

namespace Beyond8.Catalog.Application.Validators.Lessons;

public class ReorderLessonRequestValidator : AbstractValidator<ReorderLessonRequest>
{
    public ReorderLessonRequestValidator()
    {
        RuleFor(x => x.LessonId)
            .NotEmpty().WithMessage("LessonId không được để trống");

        RuleFor(x => x.NewSectionId)
            .NotEmpty().WithMessage("NewSectionId không được để trống");

        RuleFor(x => x.NewOrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("NewOrderIndex phải lớn hơn hoặc bằng 0");
    }
}