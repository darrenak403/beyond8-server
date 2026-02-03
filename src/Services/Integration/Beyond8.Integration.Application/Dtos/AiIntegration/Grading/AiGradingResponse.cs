namespace Beyond8.Integration.Application.Dtos.AiIntegration.Grading;


public class AiGradingResponse
{
    public Guid SubmissionId { get; set; }
    public decimal Score { get; set; }
    public int TotalPoints { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<CriteriaGradingResult> CriteriaResults { get; set; } = [];
    public List<string> Strengths { get; set; } = [];
    public List<string> Improvements { get; set; } = [];
    public List<string> Suggestions { get; set; } = [];
}


public class CriteriaGradingResult
{
    public string CriteriaName { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Feedback { get; set; } = string.Empty;
}
