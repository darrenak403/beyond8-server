using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Assessment.Domain.Enums;
using Beyond8.Common.Data.Base;

namespace Beyond8.Assessment.Domain.Entities;

public class AssignmentSubmission : BaseEntity
{
    public Guid StudentId { get; set; }

    public Guid AssignmentId { get; set; }


    [ForeignKey(nameof(AssignmentId))]
    public virtual Assignment Assignment { get; set; } = null!;

    public int SubmissionNumber { get; set; } = 1;

    public DateTime SubmittedAt { get; set; }

    public string? TextContent { get; set; }

    [Column(TypeName = "jsonb")]
    public string? FileUrls { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? AiScore { get; set; }

    [Column(TypeName = "jsonb")]
    public string? AiFeedback { get; set; }

    // Final Grading (by Instructor)
    [Column(TypeName = "decimal(10, 2)")]
    public decimal? FinalScore { get; set; }

    [MaxLength(2000)]
    public string? InstructorFeedback { get; set; }

    public Guid? GradedBy { get; set; }

    public DateTime? GradedAt { get; set; }

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;
}
