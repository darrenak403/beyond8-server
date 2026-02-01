using Beyond8.Assessment.Domain.Enums;
using Beyond8.Assessment.Domain.JSONFields;

namespace Beyond8.Assessment.Application.Dtos.Assignments;

public class AssignmentResponse
{
    public Guid Id { get; set; }
    public Guid InstructorId { get; set; }
    public Guid? CourseId { get; set; }
    public Guid? SectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<AssignmentAttachmentItem>? AttachmentUrls { get; set; }
    public AssignmentSubmissionType SubmissionType { get; set; }
    public List<string>? AllowedFileTypes { get; set; }
    public int MaxTextLength { get; set; }
    public GradingMode GradingMode { get; set; }
    public int TotalPoints { get; set; }
    public string? RubricUrl { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int TotalSubmissions { get; set; }
    public decimal? AverageScore { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
