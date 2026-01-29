using Beyond8.Catalog.Application.Dtos.Sections;
using FluentValidation;

namespace Beyond8.Catalog.Application.Validators.Section;

public class ReorderSectionsRequestValidator : AbstractValidator<ReorderSectionsRequest>
{
    public ReorderSectionsRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("CourseId không được để trống");

        RuleFor(x => x.Sections)
            .NotEmpty().WithMessage("Danh sách sections không được để trống")
            .Must(sections => sections.All(s => s.SectionId != Guid.Empty))
            .WithMessage("Tất cả SectionId phải hợp lệ")
            .Must(sections => sections.Select(s => s.NewOrderIndex).Distinct().Count() == sections.Count)
            .WithMessage("Các thứ tự mới phải duy nhất");
    }
}