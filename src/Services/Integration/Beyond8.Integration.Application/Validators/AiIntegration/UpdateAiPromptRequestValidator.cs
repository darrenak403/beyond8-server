using Beyond8.Integration.Application.Dtos.AiIntegration;
using FluentValidation;

namespace Beyond8.Integration.Application.Validators.AiIntegration;

public class UpdateAiPromptRequestValidator : AbstractValidator<UpdateAiPromptRequest>
{
    public UpdateAiPromptRequestValidator()
    {
        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Mô tả không được vượt quá 1000 ký tự");
        });

        When(x => x.Category != null, () =>
        {
            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Category không hợp lệ");
        });

        When(x => x.Template != null, () =>
        {
            RuleFor(x => x.Template)
                .NotEmpty().WithMessage("Template không được để trống");
        });

        When(x => x.MaxTokens != null, () =>
        {
            RuleFor(x => x.MaxTokens)
                .GreaterThan(0).WithMessage("Max tokens phải lớn hơn 0");
        });

        When(x => x.Temperature != null, () =>
        {
            RuleFor(x => x.Temperature)
                .GreaterThanOrEqualTo(0).WithMessage("Temperature phải lớn hơn hoặc bằng 0");
        });

        When(x => x.TopP != null, () =>
        {
            RuleFor(x => x.TopP)
                .GreaterThanOrEqualTo(0).WithMessage("Top P phải lớn hơn hoặc bằng 0");
        });

        When(x => x.SystemPrompt != null, () =>
        {
            RuleFor(x => x.SystemPrompt)
                .MaximumLength(2000).WithMessage("System prompt không được vượt quá 2000 ký tự");
        });
    }
}
