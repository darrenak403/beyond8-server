using Beyond8.Assessment.Application.Dtos.Reassign;
using FluentValidation;

namespace Beyond8.Assessment.Application.Validators.Reassign;

public class GetReassignOverviewRequestValidator : AbstractValidator<GetReassignOverviewRequest>
{
    public GetReassignOverviewRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber phải lớn hơn hoặc bằng 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize phải từ 1 đến 100");

        RuleFor(x => x.Search)
            .MaximumLength(200).WithMessage("Search không được quá 200 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Search));
    }
}
