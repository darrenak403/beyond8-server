using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace Beyond8.Catalog.Application.Dtos.Sections;

public class SwitchActivationRequest
{
    [Required]
    public bool IsPublished { get; set; }
}

public class SwitchActivationRequestValidator : AbstractValidator<SwitchActivationRequest>
{
    public SwitchActivationRequestValidator()
    {
        RuleFor(x => x.IsPublished)
            .NotNull().WithMessage("Trạng thái kích hoạt không được để trống");
    }
}