using FluentValidation;
using Beyond8.Catalog.Application.Dtos.Courses;

namespace Beyond8.Catalog.Application.Validators.Course;

public class UpdateCourseMetadataRequestValidator : AbstractValidator<UpdateCourseMetadataRequest>
{
    public UpdateCourseMetadataRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề khóa học không được để trống")
            .MaximumLength(200).WithMessage("Tiêu đề không được vượt quá 200 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Mô tả không được vượt quá 5000 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ShortDescription)
            .MaximumLength(1000).WithMessage("Mô tả ngắn không được vượt quá 1000 ký tự")
            .When(x => !string.IsNullOrEmpty(x.ShortDescription));

        RuleFor(x => x.Language)
            .MaximumLength(10).WithMessage("Mã ngôn ngữ không được vượt quá 10 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Language));

        RuleFor(x => x.Price)
            .InclusiveBetween(0, 100000000).WithMessage("Giá khóa học phải từ 0 đến 100 triệu VND")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.ThumbnailUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _)).WithMessage("URL thumbnail không hợp lệ")
            .When(x => !string.IsNullOrEmpty(x.ThumbnailUrl));
    }
}