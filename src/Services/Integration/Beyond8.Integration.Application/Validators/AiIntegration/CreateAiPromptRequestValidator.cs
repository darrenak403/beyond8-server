using Beyond8.Integration.Application.Dtos.AiIntegration;
using FluentValidation;

namespace Beyond8.Integration.Application.Validators.AiIntegration;

public class CreateAiPromptRequestValidator : AbstractValidator<CreateAiPromptRequest>
{
    public CreateAiPromptRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên prompt không được để trống")
            .MaximumLength(200).WithMessage("Tên prompt không được vượt quá 200 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Mô tả không được vượt quá 1000 ký tự");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Category không hợp lệ");

        RuleFor(x => x.Template)
            .NotEmpty().WithMessage("Template không được để trống");

        RuleFor(x => x.Version)
            .NotEmpty().WithMessage("Version không được để trống")
            .MaximumLength(50).WithMessage("Version không được vượt quá 50 ký tự")
            .Matches(@"^\d+\.\d+\.\d+$").WithMessage("Version phải theo định dạng x.y.z (ví dụ: 1.0.0)");

        RuleFor(x => x.MaxTokens)
            .GreaterThan(0).WithMessage("Max tokens phải lớn hơn 0");

        RuleFor(x => x.Temperature)
            .GreaterThanOrEqualTo(0).WithMessage("Temperature phải lớn hơn hoặc bằng 0");

        RuleFor(x => x.TopP)
            .GreaterThanOrEqualTo(0).WithMessage("Top P phải lớn hơn hoặc bằng 0");

        RuleFor(x => x.SystemPrompt)
            .MaximumLength(2000).WithMessage("System prompt không được vượt quá 2000 ký tự");
    }
}
