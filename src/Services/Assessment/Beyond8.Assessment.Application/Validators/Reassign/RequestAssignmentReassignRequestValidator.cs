using Beyond8.Assessment.Application.Dtos.Reassign;
using FluentValidation;

namespace Beyond8.Assessment.Application.Validators.Reassign;

public class RequestAssignmentReassignRequestValidator : AbstractValidator<RequestAssignmentReassignRequest>
{
    public RequestAssignmentReassignRequestValidator()
    {
        RuleFor(x => x.Reason)
            .IsInEnum().WithMessage("Lý do không hợp lệ.");

        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage("Ghi chú không được quá 500 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Note));
    }
}
