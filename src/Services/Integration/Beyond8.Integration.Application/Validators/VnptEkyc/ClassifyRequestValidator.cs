using Beyond8.Integration.Application.Dtos.VnptEkyc;
using FluentValidation;

namespace Beyond8.Integration.Application.Validators.VnptEkyc;

public class ClassifyRequestValidator : AbstractValidator<ClassifyRequest>
{
    public ClassifyRequestValidator()
    {
        RuleFor(x => x.Img)
            .NotEmpty().WithMessage("Image hash không được để trống");

        RuleFor(x => x.CardType)
            .Must(x => x == 2 || x == 3)
            .WithMessage("Card type phải là 2 (mặt trước mới) hoặc 3 (mặt sau mới)");
    }
}
