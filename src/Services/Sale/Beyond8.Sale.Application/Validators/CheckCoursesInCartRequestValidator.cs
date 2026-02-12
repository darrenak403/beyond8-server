using Beyond8.Sale.Application.Dtos.Carts;
using FluentValidation;

namespace Beyond8.Sale.Application.Validators;

public class CheckCoursesInCartRequestValidator : AbstractValidator<CheckCoursesInCartRequest>
{
    public CheckCoursesInCartRequestValidator()
    {
        RuleFor(x => x.CourseIds)
            .NotNull().WithMessage("Danh sách khóa học không được null")
            .Must(ids => ids.Count > 0 && ids.Count <= 100)
            .WithMessage("Số lượng khóa học phải từ 1 đến 100");

        RuleFor(x => x.CourseIds)
            .Must(ids => ids.All(id => id != Guid.Empty))
            .WithMessage("CourseId không được rỗng");
    }
}
