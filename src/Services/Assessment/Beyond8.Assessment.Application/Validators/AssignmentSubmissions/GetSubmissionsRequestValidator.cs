using Beyond8.Assessment.Application.Dtos.AssignmentSubmissions;
using FluentValidation;

namespace Beyond8.Assessment.Application.Validators.AssignmentSubmissions;

public class GetSubmissionsRequestValidator : AbstractValidator<GetSubmissionsRequest>
{
    public GetSubmissionsRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber phải lớn hơn hoặc bằng 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize phải từ 1 đến 100");
    }
}
