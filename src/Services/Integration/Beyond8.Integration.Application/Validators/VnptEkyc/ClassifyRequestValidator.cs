using Beyond8.Integration.Application.Dtos.VnptEkyc;
using FluentValidation;

namespace Beyond8.Integration.Application.Validators.VnptEkyc
{
    public class ClassifyRequestValidator : AbstractValidator<ClassifyRequest>
    {
        public ClassifyRequestValidator()
        {
            RuleFor(x => x.Img)
                .NotEmpty().WithMessage("Image hash không được để trống");
        }
    }
}
