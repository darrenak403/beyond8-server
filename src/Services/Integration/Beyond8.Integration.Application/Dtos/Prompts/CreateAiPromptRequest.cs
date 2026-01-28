using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Application.Dtos.Prompts
{
    public class CreateAiPromptRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public PromptCategory Category { get; set; }
        public string Template { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public Dictionary<string, object>? Variables { get; set; }
        public Dictionary<string, object>? DefaultParameters { get; set; }
        public string? SystemPrompt { get; set; }
        public int MaxTokens { get; set; } = 1000;
        public decimal Temperature { get; set; } = 0.7m;
        public decimal TopP { get; set; } = 0.9m;
        public List<string>? Tags { get; set; }
    }
}
