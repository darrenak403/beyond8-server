using FluentValidation;
using Beyond8.Catalog.Application.Dtos.Courses;

namespace Beyond8.Catalog.Application.Validators.Course;

public class UpdateCourseContentRequestValidator : AbstractValidator<UpdateCourseContentRequest>
{
    public UpdateCourseContentRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Mô tả khóa học không được để trống");
    }
}