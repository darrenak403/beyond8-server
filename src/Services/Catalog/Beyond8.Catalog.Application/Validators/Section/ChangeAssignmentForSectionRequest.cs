using Beyond8.Catalog.Application.Dtos.Sections;
using FluentValidation;

namespace Beyond8.Catalog.Application.Validators.Section;

public class ChangeAssignmentForSectionRequestValidator : AbstractValidator<ChangeAssignmentForSectionRequest>
{
    public ChangeAssignmentForSectionRequestValidator()
    {
        RuleFor(x => x.AssignmentId)
            .NotEmpty().WithMessage("AssignmentId không được để trống");
    }
}