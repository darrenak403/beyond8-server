namespace Beyond8.Integration.Application.Dtos.AiIntegration.Grading;

public class AiGradingRequest
{
    public Guid SubmissionId { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }
    public string AssignmentTitle { get; set; } = string.Empty;
    public string AssignmentDescription { get; set; } = string.Empty;
    public string? TextContent { get; set; }
    public List<string>? FileUrls { get; set; }
    public string? RubricUrl { get; set; }
    public int TotalPoints { get; set; }
}
