using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Application.Dtos.Prompts
{
    public class UpdateAiPromptRequest
    {
        public string? Description { get; set; }
        public PromptCategory? Category { get; set; }
        public string? Template { get; set; }
        public Dictionary<string, object>? Variables { get; set; }
        public Dictionary<string, object>? DefaultParameters { get; set; }
        public string? SystemPrompt { get; set; }
        public int? MaxTokens { get; set; }
        public decimal? Temperature { get; set; }
        public decimal? TopP { get; set; }
        public bool? IsActive { get; set; }
        public List<string>? Tags { get; set; }
    }
}
