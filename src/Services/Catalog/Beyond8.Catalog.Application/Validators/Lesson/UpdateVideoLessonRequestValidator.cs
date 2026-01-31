using Beyond8.Catalog.Application.Dtos.Lessons;
using FluentValidation;

namespace Beyond8.Catalog.Application.Validators.Lesson;

public class UpdateVideoLessonRequestValidator : AbstractValidator<UpdateVideoLessonRequest>
{
    public UpdateVideoLessonRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id bài học không được để trống");

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id bài học không được để trống");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề bài học không được để trống")
            .MaximumLength(200).WithMessage("Tiêu đề bài học không được vượt quá 200 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả bài học không được vượt quá 1000 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Description));

        // Video-specific validation
        RuleFor(x => x.VideoOriginalUrl)
            .MaximumLength(500).WithMessage("URL video gốc không được vượt quá 500 ký tự")
            .When(x => !string.IsNullOrEmpty(x.VideoOriginalUrl));

        RuleFor(x => x.VideoThumbnailUrl)
            .MaximumLength(500).WithMessage("URL thumbnail không được vượt quá 500 ký tự")
            .When(x => !string.IsNullOrEmpty(x.VideoThumbnailUrl));

        RuleFor(x => x.DurationSeconds)
            .GreaterThan(0).WithMessage("Thời gian video phải lớn hơn 0")
            .When(x => x.DurationSeconds.HasValue);
    }
}