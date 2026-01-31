using Beyond8.Catalog.Application.Dtos.Sections;
using FluentValidation;

namespace Beyond8.Catalog.Application.Validators.Section;

public class UpdateSectionAssignmentRequestValidator : AbstractValidator<UpdateSectionAssignmentRequest>
{
    public UpdateSectionAssignmentRequestValidator()
    {
        RuleFor(x => x.AssignmentId)
            .NotEmpty().WithMessage("AssignmentId không được để trống");
    }
}