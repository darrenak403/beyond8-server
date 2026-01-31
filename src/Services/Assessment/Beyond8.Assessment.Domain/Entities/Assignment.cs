using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Assessment.Domain.Enums;
using Beyond8.Common.Data.Base;

namespace Beyond8.Assessment.Domain.Entities;

public class Assignment : BaseEntity
{
    public Guid InstructorId { get; set; }

    public Guid? CourseId { get; set; }

    public Guid? SectionId { get; set; }


    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// Attached files for the assignment (JSONB)
    /// Format: ["url1", "url2"]
    [Column(TypeName = "jsonb")]

    public string? AttachmentUrls { get; set; }

    public AssignmentSubmissionType SubmissionType { get; set; } = AssignmentSubmissionType.File;

    /// Allowed file types for file submission (JSONB)
    /// Format: ["pdf", "docx", "zip"]
    [Column(TypeName = "jsonb")]
    public string? AllowedFileTypes { get; set; }

    public int MaxTextLength { get; set; } = 1000;

    public GradingMode GradingMode { get; set; } = GradingMode.Manual;

    public int TotalPoints { get; set; } = 100;

    /// Rubric for grading (JSONB)
    /// Format: [{"name": "Criteria 1", "description": "...", "maxPoints": 25, "gradingGuidelines": "..."}]
    [Column(TypeName = "jsonb")]

    public string? Rubric { get; set; }

    public string? RubricUrl { get; set; }

    public int? TimeLimitMinutes { get; set; } = 60;

    public int TotalSubmissions { get; set; } = 0;

    [Column(TypeName = "decimal(5, 2)")]

    public decimal? AverageScore { get; set; }

    public virtual ICollection<AssignmentSubmission> Submissions { get; set; } = [];
}
