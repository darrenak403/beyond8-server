using Beyond8.Integration.Application.Dtos.MediaFiles;
using FluentValidation;

namespace Beyond8.Integration.Application.Validators.MediaFiles;

public class ConfirmUploadRequestValidator : AbstractValidator<ConfirmUploadRequest>
{
    public ConfirmUploadRequestValidator()
    {
        RuleFor(x => x.FileId)
            .NotEmpty().WithMessage("File ID không được để trống");
    }
}
