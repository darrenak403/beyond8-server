using Beyond8.Learning.Application.Dtos.Enrollments;
using FluentValidation;

namespace Beyond8.Learning.Application.Validators;

public class EnrollFreeRequestValidator : AbstractValidator<EnrollFreeRequest>
{
    public EnrollFreeRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty()
            .WithMessage("Mã khóa học không được để trống.");
    }
}
