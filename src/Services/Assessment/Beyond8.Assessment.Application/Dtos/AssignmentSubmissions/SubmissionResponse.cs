using Beyond8.Assessment.Domain.Enums;

namespace Beyond8.Assessment.Application.Dtos.AssignmentSubmissions;

public class SubmissionResponse
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid AssignmentId { get; set; }
    public int SubmissionNumber { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string? TextContent { get; set; }
    public List<string>? FileUrls { get; set; }
    public decimal? AiScore { get; set; }
    public string? AiFeedback { get; set; }
    public decimal? FinalScore { get; set; }
    public string? InstructorFeedback { get; set; }
    public Guid? GradedBy { get; set; }
    public DateTime? GradedAt { get; set; }
    public SubmissionStatus Status { get; set; }
    public AssignmentSubmissionType SubmissionType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
