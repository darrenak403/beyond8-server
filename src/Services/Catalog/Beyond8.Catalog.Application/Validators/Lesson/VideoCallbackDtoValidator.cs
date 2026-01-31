using Beyond8.Catalog.Application.Dtos.Lessons;
using FluentValidation;

namespace Beyond8.Catalog.Application.Validators.Lesson;

public class VideoCallbackDtoValidator : AbstractValidator<VideoCallbackDto>
{
    public VideoCallbackDtoValidator()
    {
        RuleFor(x => x.OriginalKey)
            .NotEmpty().WithMessage("OriginalKey không được để trống")
            .MaximumLength(500).WithMessage("OriginalKey không được vượt quá 500 ký tự");

        RuleFor(x => x.TranscodingData)
            .NotNull().WithMessage("Dữ liệu transcoding không được để trống");

        RuleFor(x => x.TranscodingData.Variants)
            .NotNull().WithMessage("Danh sách variants không được để trống")
            .When(x => x.TranscodingData != null);
    }
}
