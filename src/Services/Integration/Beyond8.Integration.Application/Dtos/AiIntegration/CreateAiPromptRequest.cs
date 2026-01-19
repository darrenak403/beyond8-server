using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Application.Dtos.AiIntegration;
public class CreateAiPromptRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PromptCategory Category { get; set; }
    public string Template { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? Variables { get; set; }
    public string? DefaultParameters { get; set; }
    public string? SystemPrompt { get; set; }
    public int MaxTokens { get; set; } = 1000;
    public decimal Temperature { get; set; } = 0.7m;
    public decimal TopP { get; set; } = 0.9m;
    public string? Tags { get; set; }
}
