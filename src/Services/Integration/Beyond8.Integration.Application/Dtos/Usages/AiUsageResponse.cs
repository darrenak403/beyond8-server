using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Application.Dtos.Usages
{
    public class AiUsageResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public AiProvider Provider { get; set; }
        public string Model { get; set; } = string.Empty;
        public AiOperation Operation { get; set; }
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
        public int TotalTokens { get; set; }
        public decimal InputCost { get; set; }
        public decimal OutputCost { get; set; }
        public decimal TotalCost { get; set; }
        public Guid? PromptId { get; set; }
        public string? RequestSummary { get; set; }
        public int ResponseTimeMs { get; set; }
        public AiUsageStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
