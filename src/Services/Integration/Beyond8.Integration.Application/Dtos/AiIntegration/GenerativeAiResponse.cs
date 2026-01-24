namespace Beyond8.Integration.Application.Dtos.AiIntegration;

public class GenerativeAiResponse
{
    public string Content { get; set; } = string.Empty;
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int TotalTokens { get; set; }
    public decimal InputCost { get; set; }
    public decimal OutputCost { get; set; }
    public decimal TotalCost { get; set; }
    public int ResponseTimeMs { get; set; }
    public string Model { get; set; } = string.Empty;
}
