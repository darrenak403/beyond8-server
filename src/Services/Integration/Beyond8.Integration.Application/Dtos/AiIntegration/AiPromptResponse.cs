using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Application.Dtos.AiIntegration;

public class AiPromptResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PromptCategory Category { get; set; }
    public string Template { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Dictionary<string, object>? Variables { get; set; }
    public Dictionary<string, object>? DefaultParameters { get; set; }
    public string? SystemPrompt { get; set; }
    public int MaxTokens { get; set; }
    public decimal Temperature { get; set; }
    public decimal TopP { get; set; }
    public List<string>? Tags { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
