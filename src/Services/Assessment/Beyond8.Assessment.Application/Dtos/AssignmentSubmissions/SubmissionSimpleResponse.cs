using Beyond8.Assessment.Domain.Enums;

namespace Beyond8.Assessment.Application.Dtos.AssignmentSubmissions;

public class SubmissionSimpleResponse
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid? SectionId { get; set; }
    public int SubmissionNumber { get; set; }
    public DateTime SubmittedAt { get; set; }
    public decimal? AiScore { get; set; }
    public decimal? FinalScore { get; set; }
    public SubmissionStatus Status { get; set; }
    public AssignmentSubmissionType SubmissionType { get; set; }
    public DateTime CreatedAt { get; set; }
}
