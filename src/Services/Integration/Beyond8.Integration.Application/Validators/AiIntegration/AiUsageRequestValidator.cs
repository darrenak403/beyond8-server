using Beyond8.Integration.Application.Dtos.AiIntegration;
using Beyond8.Integration.Domain.Enums;
using FluentValidation;

namespace Beyond8.Integration.Application.Validators.AiIntegration;

public class AiUsageRequestValidator : AbstractValidator<AiUsageRequest>
{
    public AiUsageRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID không được để trống");

        RuleFor(x => x.Provider)
            .IsInEnum().WithMessage("Provider không hợp lệ");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model không được để trống")
            .MaximumLength(100).WithMessage("Model không được vượt quá 100 ký tự");

        RuleFor(x => x.Operation)
            .IsInEnum().WithMessage("Operation không hợp lệ");

        RuleFor(x => x.InputTokens)
            .GreaterThanOrEqualTo(0).WithMessage("Input tokens phải lớn hơn hoặc bằng 0");

        RuleFor(x => x.OutputTokens)
            .GreaterThanOrEqualTo(0).WithMessage("Output tokens phải lớn hơn hoặc bằng 0");

        RuleFor(x => x.InputCost)
            .GreaterThanOrEqualTo(0).WithMessage("Input cost phải lớn hơn hoặc bằng 0");

        RuleFor(x => x.OutputCost)
            .GreaterThanOrEqualTo(0).WithMessage("Output cost phải lớn hơn hoặc bằng 0");

        RuleFor(x => x.RequestSummary)
            .MaximumLength(500).WithMessage("Request summary không được vượt quá 500 ký tự");

        RuleFor(x => x.ResponseTimeMs)
            .GreaterThanOrEqualTo(0).WithMessage("Response time phải lớn hơn hoặc bằng 0");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status không hợp lệ");

        RuleFor(x => x.ErrorMessage)
            .MaximumLength(1000).WithMessage("Error message không được vượt quá 1000 ký tự");
    }
}
