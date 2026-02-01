using Beyond8.Catalog.Application.Dtos.CourseDocuments;
using FluentValidation;

namespace Beyond8.Catalog.Application.Validators.CourseDocument;

public class CreateCourseDocumentRequestValidator : AbstractValidator<CreateCourseDocumentRequest>
{
    public CreateCourseDocumentRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("CourseId không được để trống");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề không được để trống")
            .MaximumLength(200).WithMessage("Tiêu đề không được vượt quá 200 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự");

        RuleFor(x => x.CourseDocumentUrl)
            .NotEmpty().WithMessage("URL tài liệu không được để trống")
            .Must(BeValidUrl).WithMessage("URL tài liệu không hợp lệ");
    }

    private bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}

public class UpdateCourseDocumentRequestValidator : AbstractValidator<UpdateCourseDocumentRequest>
{
    public UpdateCourseDocumentRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề không được để trống")
            .MaximumLength(200).WithMessage("Tiêu đề không được vượt quá 200 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự");

        RuleFor(x => x.CourseDocumentUrl)
            .NotEmpty().WithMessage("URL tài liệu không được để trống")
            .Must(BeValidUrl).WithMessage("URL tài liệu không hợp lệ");
    }

    private bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}