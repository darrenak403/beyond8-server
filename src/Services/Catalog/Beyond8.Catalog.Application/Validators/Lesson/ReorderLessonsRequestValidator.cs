using Beyond8.Catalog.Application.Dtos.Lessons;
using FluentValidation;

namespace Beyond8.Catalog.Application.Validators.Lesson;

public class ReorderLessonsRequestValidator : AbstractValidator<ReorderLessonsRequest>
{
    public ReorderLessonsRequestValidator()
    {
        RuleFor(x => x.SectionId)
            .NotEmpty().WithMessage("SectionId không được để trống");

        RuleFor(x => x.Lessons)
            .NotEmpty().WithMessage("Danh sách lessons không được để trống")
            .Must(lessons => lessons.All(l => l.LessonId != Guid.Empty))
            .WithMessage("Tất cả LessonId phải hợp lệ")
            .Must(lessons => lessons.Select(l => l.NewOrderIndex).Distinct().Count() == lessons.Count)
            .WithMessage("Các thứ tự mới phải duy nhất");
    }
}