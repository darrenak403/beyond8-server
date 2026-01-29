using Beyond8.Catalog.Application.Dtos.Sections;
using FluentValidation;

namespace Beyond8.Catalog.Application.Validators.Section;

public class UpdateSectionRequestValidator : AbstractValidator<UpdateSectionRequest>
{
    public UpdateSectionRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề chương không được để trống")
            .MaximumLength(200).WithMessage("Tiêu đề chương không được vượt quá 200 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả chương không được vượt quá 1000 ký tự");
    }
}