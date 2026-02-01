using Beyond8.Catalog.Application.Dtos.LessonDocuments;
using FluentValidation;

namespace Beyond8.Catalog.Application.Validators.LessonDocument;

public class CreateLessonDocumentRequestValidator : AbstractValidator<CreateLessonDocumentRequest>
{
    public CreateLessonDocumentRequestValidator()
    {
        RuleFor(x => x.LessonId)
            .NotEmpty().WithMessage("LessonId không được để trống");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề không được để trống")
            .MaximumLength(200).WithMessage("Tiêu đề không được vượt quá 200 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự");

        RuleFor(x => x.LessonDocumentUrl)
            .NotEmpty().WithMessage("URL tài liệu không được để trống")
            .Must(BeValidUrl).WithMessage("URL tài liệu không hợp lệ");
    }

    private bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}

public class UpdateLessonDocumentRequestValidator : AbstractValidator<UpdateLessonDocumentRequest>
{
    public UpdateLessonDocumentRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề không được để trống")
            .MaximumLength(200).WithMessage("Tiêu đề không được vượt quá 200 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự");

        RuleFor(x => x.LessonDocumentUrl)
            .NotEmpty().WithMessage("URL tài liệu không được để trống")
            .Must(BeValidUrl).WithMessage("URL tài liệu không hợp lệ");
    }

    private bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}