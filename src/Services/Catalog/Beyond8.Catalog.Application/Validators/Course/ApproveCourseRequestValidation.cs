using System;
using Beyond8.Catalog.Application.Dtos.Courses;
using FluentValidation;

namespace Beyond8.Catalog.Application.Validators.Course;

public class ApproveCourseRequestValidation : AbstractValidator<ApproveCourseRequest>
{
    public ApproveCourseRequestValidation()
    {
        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Ghi chú không được vượt quá 1000 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}