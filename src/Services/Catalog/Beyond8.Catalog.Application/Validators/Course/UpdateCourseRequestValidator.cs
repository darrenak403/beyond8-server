using FluentValidation;
using Beyond8.Catalog.Application.Dtos.Courses;

namespace Beyond8.Catalog.Application.Validators.Course;

public class UpdateCourseRequestValidator : AbstractValidator<UpdateCourseRequest>
{
    public UpdateCourseRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề khóa học không được để trống")
            .MaximumLength(200).WithMessage("Tiêu đề không được vượt quá 200 ký tự");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Mô tả khóa học không được để trống");

        RuleFor(x => x.ShortDescription)
            .MaximumLength(1000).WithMessage("Mô tả ngắn không được vượt quá 1000 ký tự")
            .When(x => !string.IsNullOrEmpty(x.ShortDescription));

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Danh mục không được để trống");

        RuleFor(x => x.Level)
            .IsInEnum().WithMessage("Cấp độ khóa học không hợp lệ");

        RuleFor(x => x.Language)
            .MaximumLength(10).WithMessage("Mã ngôn ngữ không được vượt quá 10 ký tự");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Giá khóa học phải lớn hơn hoặc bằng 0")
            .LessThanOrEqualTo(100000000).WithMessage("Giá khóa học không được vượt quá 100 triệu VND");

        RuleFor(x => x.ThumbnailUrl)
            .NotEmpty().WithMessage("URL thumbnail không được để trống")
            .Must(BeValidUrl).WithMessage("URL thumbnail không hợp lệ");

        RuleFor(x => x.Outcomes)
            .NotEmpty().WithMessage("Phải có ít nhất một mục tiêu học tập");

        RuleForEach(x => x.Outcomes)
            .NotEmpty().WithMessage("Mục tiêu học tập không được để trống")
            .MaximumLength(500).WithMessage("Mục tiêu học tập không được vượt quá 500 ký tự");

        RuleForEach(x => x.Requirements)
            .MaximumLength(500).WithMessage("Yêu cầu không được vượt quá 500 ký tự")
            .When(x => x.Requirements != null);

        RuleForEach(x => x.TargetAudience)
            .MaximumLength(500).WithMessage("Đối tượng học viên không được vượt quá 500 ký tự")
            .When(x => x.TargetAudience != null);
    }

    private bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}