using Beyond8.Integration.Application.Dtos;
using FluentValidation;

namespace Beyond8.Integration.Application.Validators;

public class ConfirmUploadRequestValidator : AbstractValidator<ConfirmUploadRequest>
{
    public ConfirmUploadRequestValidator()
    {
        RuleFor(x => x.FileId)
            .NotEmpty().WithMessage("File ID không được để trống");
    }
}
