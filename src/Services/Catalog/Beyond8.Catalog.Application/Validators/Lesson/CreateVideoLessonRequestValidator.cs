using Beyond8.Catalog.Application.Dtos.Lessons;
using FluentValidation;

namespace Beyond8.Catalog.Application.Validators.Lesson;

public class CreateVideoLessonRequestValidator : AbstractValidator<CreateVideoLessonRequest>
{
    public CreateVideoLessonRequestValidator()
    {
        RuleFor(x => x.SectionId)
            .NotEmpty().WithMessage("SectionId không được để trống");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề bài học không được để trống")
            .MaximumLength(200).WithMessage("Tiêu đề bài học không được vượt quá 200 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả bài học không được vượt quá 1000 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Description));

        // Video-specific validation
        RuleFor(x => x.VideoOriginalUrl)
            .NotEmpty().WithMessage("URL video gốc không được để trống cho bài học video")
            .MaximumLength(500).WithMessage("URL video gốc không được vượt quá 500 ký tự");

        RuleFor(x => x.VideoThumbnailUrl)
            .MaximumLength(500).WithMessage("URL thumbnail không được vượt quá 500 ký tự");

        RuleFor(x => x.DurationSeconds)
            .NotNull().WithMessage("Thời gian video không được để trống")
            .GreaterThan(0).WithMessage("Thời gian video phải lớn hơn 0");
    }
}