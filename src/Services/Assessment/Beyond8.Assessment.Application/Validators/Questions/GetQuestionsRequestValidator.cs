using Beyond8.Assessment.Application.Dtos.Questions;
using FluentValidation;

namespace Beyond8.Assessment.Application.Validators.Questions;

public class GetQuestionsRequestValidator : AbstractValidator<GetQuestionsRequest>
{
    public GetQuestionsRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber phải lớn hơn hoặc bằng 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize phải từ 1 đến 100");

        RuleFor(x => x.Tag)
            .MaximumLength(100).WithMessage("Tag không được vượt quá 100 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Tag));
    }
}
