using Beyond8.Catalog.Application.Dtos.Sections;
using FluentValidation;

namespace Beyond8.Catalog.Application.Validators.Section;

public class CreateSectionRequestValidator : AbstractValidator<CreateSectionRequest>
{
    public CreateSectionRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("CourseId không được để trống");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề chương không được để trống")
            .MaximumLength(200).WithMessage("Tiêu đề chương không được vượt quá 200 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả chương không được vượt quá 1000 ký tự");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("Thứ tự phải lớn hơn hoặc bằng 0");

        RuleFor(x => x.AssignmentId)
            .NotEmpty().When(x => x.AssignmentId.HasValue)
            .WithMessage("AssignmentId phải là GUID hợp lệ nếu được cung cấp");
    }
}