using Beyond8.Assessment.Domain.Enums;

namespace Beyond8.Assessment.Application.Dtos.Assignments;

public class UpdateAssignmentRequest
{
    public Guid? CourseId { get; set; }
    public Guid? SectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string>? AttachmentUrls { get; set; }
    public AssignmentSubmissionType SubmissionType { get; set; } = AssignmentSubmissionType.File;
    public List<string>? AllowedFileTypes { get; set; }
    public int MaxTextLength { get; set; } = 1000;
    public GradingMode GradingMode { get; set; } = GradingMode.AiAssisted;
    public int TotalPoints { get; set; } = 100;
    public string? RubricUrl { get; set; }
    public int? TimeLimitMinutes { get; set; } = 60;
}
