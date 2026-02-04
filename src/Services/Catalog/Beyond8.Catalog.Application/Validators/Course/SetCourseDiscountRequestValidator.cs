using FluentValidation;
using Beyond8.Catalog.Application.Dtos.Courses;

namespace Beyond8.Catalog.Application.Validators.Course;

public class SetCourseDiscountRequestValidator : AbstractValidator<SetCourseDiscountRequest>
{
    public SetCourseDiscountRequestValidator()
    {
        RuleFor(x => x.DiscountPercent)
            .InclusiveBetween(0, 100).WithMessage("Phần trăm giảm giá phải từ 0 đến 100")
            .When(x => x.DiscountPercent.HasValue);

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Số tiền giảm không được âm")
            .When(x => x.DiscountAmount.HasValue);
    }
}
