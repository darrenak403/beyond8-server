using Beyond8.Catalog.Application.Dtos.Lessons;
using FluentValidation;

namespace Beyond8.Catalog.Application.Validators.Lessons;

public class ReorderSectionRequestValidator : AbstractValidator<ReorderSectionRequest>
{
    public ReorderSectionRequestValidator()
    {
        RuleFor(x => x.SectionId)
            .NotEmpty().WithMessage("SectionId không được để trống");

        RuleFor(x => x.NewOrderIndex)
            .GreaterThan(0).WithMessage("NewOrderIndex phải lớn hơn 0");
    }
}