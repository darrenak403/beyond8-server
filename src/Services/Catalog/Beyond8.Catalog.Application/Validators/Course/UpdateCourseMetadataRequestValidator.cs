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

        RuleFor(x => x.ShortDescription)
            .MaximumLength(1000).WithMessage("Mô tả ngắn không được vượt quá 1000 ký tự");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Danh mục không được để trống");

        RuleFor(x => x.Language)
            .MaximumLength(10).WithMessage("Mã ngôn ngữ không được vượt quá 10 ký tự");

        RuleFor(x => x.Price)
            .InclusiveBetween(0, 100000000).WithMessage("Giá khóa học phải từ 0 đến 100 triệu VND");

        RuleFor(x => x.ThumbnailUrl)
            .NotEmpty().WithMessage("URL thumbnail không được để trống")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _)).WithMessage("URL thumbnail không hợp lệ");
    }
}