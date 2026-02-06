using Beyond8.Learning.Application.Dtos.CourseReview;
using FluentValidation;

namespace Beyond8.Learning.Application.Validators.CourseReview;

public class GetCourseReviewsRequestValidator : AbstractValidator<GetCourseReviewsRequest>
{
    public GetCourseReviewsRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty()
            .WithMessage("Mã khóa học không được để trống.");

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Số trang phải lớn hơn hoặc bằng 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Kích thước trang phải từ 1 đến 100.");
    }
}
