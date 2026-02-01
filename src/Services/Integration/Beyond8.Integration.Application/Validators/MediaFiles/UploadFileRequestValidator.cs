using Beyond8.Integration.Application.Dtos.MediaFiles;
using FluentValidation;

namespace Beyond8.Integration.Application.Validators.MediaFiles
{
    public class UploadFileRequestValidator : AbstractValidator<UploadFileRequest>
    {
        private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/jpg", "image/png", "image/webp"];
        private static readonly string[] AllowedDocumentTypes = ["application/pdf", "image/jpeg", "image/jpg", "image/png"];
        private static readonly string[] AllowedVideoTypes = ["video/mp4", "video/mov", "video/avi", "video/wmv", "video/flv", "video/mkv"];

        public UploadFileRequestValidator()
        {
            RuleFor(x => x.FileName)
                .NotEmpty().WithMessage("Tên file không được để trống")
                .MaximumLength(500).WithMessage("Tên file không được vượt quá 500 ký tự");

            RuleFor(x => x.ContentType)
                .NotEmpty().WithMessage("Content type không được để trống")
                .MaximumLength(200).WithMessage("Content type không được vượt quá 200 ký tự");

            RuleFor(x => x.Size)
                .GreaterThan(0).WithMessage("Kích thước file phải lớn hơn 0");
            // Note: Size limit is set in specific validator factories (ForAvatar, ForCourseVideo, etc.)
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

        public static UploadFileRequestValidator ForCover()
        {
            var validator = new UploadFileRequestValidator();

            validator.RuleFor(x => x.ContentType)
                .Must(ct => AllowedImageTypes.Contains(ct.ToLower()))
                .WithMessage($"Cover chỉ chấp nhận định dạng: {string.Join(", ", AllowedImageTypes)}");

            validator.RuleFor(x => x.Size)
                .LessThanOrEqualTo(5242880).WithMessage("Kích thước cover không được vượt quá 5MB");

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

        public static UploadFileRequestValidator ForIntroVideo()
        {
            var validator = new UploadFileRequestValidator();

            validator.RuleFor(x => x.ContentType)
                .Must(ct => AllowedVideoTypes.Contains(ct.ToLower()))
                .WithMessage("Video chỉ chấp nhận định dạng: mp4, mov, avi, wmv, flv, mkv, webm");

            validator.RuleFor(x => x.Size)
                .LessThanOrEqualTo(5242880).WithMessage("Kích thước video không được vượt quá 5MB");

            return validator;
        }

        public static UploadFileRequestValidator ForCourseVideo()
        {
            var validator = new UploadFileRequestValidator();

            validator.RuleFor(x => x.ContentType)
                .Must(ct => AllowedVideoTypes.Contains(ct.ToLower()))
                .WithMessage("Video chỉ chấp nhận định dạng: mp4, mov, avi, wmv, flv, mkv, webm");

            validator.RuleFor(x => x.Size)
                .LessThanOrEqualTo(2147483648).WithMessage("Kích thước video không được vượt quá 2GB");

            return validator;
        }

        public static IValidator<UploadFileRequest> ForCourseDocument()
        {
            var validator = new UploadFileRequestValidator();

            validator.RuleFor(x => x.ContentType)
                .Must(ct => AllowedDocumentTypes.Contains(ct.ToLower()))
                .WithMessage($"Tài liệu chỉ chấp nhận định dạng: {string.Join(", ", AllowedDocumentTypes)}");

            validator.RuleFor(x => x.Size)
                .LessThanOrEqualTo(1073741824).WithMessage($"Kích thước tài liệu không được vượt quá 1GB");

            return validator;
        }

        public static IValidator<UploadFileRequest> ForAssignmentSubmission()
        {
            var validator = new UploadFileRequestValidator();

            validator.RuleFor(x => x.Size)
                .LessThanOrEqualTo(20971520).WithMessage($"Kích thước tài liệu không được vượt quá 20MB");
            return validator;
        }

        public static IValidator<UploadFileRequest> ForAssignmentRubric()
        {
            var validator = new UploadFileRequestValidator();

            validator.RuleFor(x => x.ContentType)
                .Must(ct => AllowedDocumentTypes.Contains(ct.ToLower()))
                .WithMessage($"Rubric chỉ chấp nhận định dạng: {string.Join(", ", AllowedDocumentTypes)}");

            validator.RuleFor(x => x.Size)
                .LessThanOrEqualTo(104857600).WithMessage($"Kích thước rubric không được vượt quá 100MB");

            return validator;
        }
    }
}