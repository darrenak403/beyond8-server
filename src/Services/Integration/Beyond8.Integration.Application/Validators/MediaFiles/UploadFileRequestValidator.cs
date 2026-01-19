using Beyond8.Integration.Application.Dtos.MediaFiles;
using FluentValidation;

namespace Beyond8.Integration.Application.Validators.MediaFiles;

public class UploadFileRequestValidator : AbstractValidator<UploadFileRequest>
{
    private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/jpg", "image/png", "image/webp"];
    private static readonly string[] AllowedDocumentTypes = ["application/pdf", "image/jpeg", "image/jpg", "image/png"];

    public UploadFileRequestValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("Tên file không được để trống")
            .MaximumLength(500).WithMessage("Tên file không được vượt quá 500 ký tự");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type không được để trống")
            .MaximumLength(200).WithMessage("Content type không được vượt quá 200 ký tự");

        RuleFor(x => x.Size)
            .GreaterThan(0).WithMessage("Kích thước file phải lớn hơn 0")
            .LessThanOrEqualTo(10485760).WithMessage("Kích thước file không được vượt quá 10MB");
    }

    public static UploadFileRequestValidator ForAvatar()
    {
        var validator = new UploadFileRequestValidator();

        validator.RuleFor(x => x.ContentType)
            .Must(ct => AllowedImageTypes.Contains(ct.ToLower()))
            .WithMessage($"Avatar chỉ chấp nhận định dạng: {string.Join(", ", AllowedImageTypes)}");

        validator.RuleFor(x => x.Size)
            .LessThanOrEqualTo(5242880).WithMessage("Kích thước avatar không được vượt quá 5MB");

        return validator;
    }

    public static UploadFileRequestValidator ForDocument()
    {
        var validator = new UploadFileRequestValidator();

        validator.RuleFor(x => x.ContentType)
            .Must(ct => AllowedDocumentTypes.Contains(ct.ToLower()))
            .WithMessage($"Tài liệu chỉ chấp nhận định dạng: {string.Join(", ", AllowedDocumentTypes)}");

        validator.RuleFor(x => x.Size)
            .LessThanOrEqualTo(10485760).WithMessage("Kích thước tài liệu không được vượt quá 10MB");

        return validator;
    }
}
