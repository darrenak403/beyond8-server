using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Application.Dtos.AiIntegration;

public class UpdateAiPromptRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public PromptCategory? Category { get; set; }
    public string? Template { get; set; }
    public string? Variables { get; set; }
    public string? DefaultParameters { get; set; }
    public string? SystemPrompt { get; set; }
    public int? MaxTokens { get; set; }
    public decimal? Temperature { get; set; }
    public decimal? TopP { get; set; }
    public bool? IsActive { get; set; }
    public string? Tags { get; set; }
}
