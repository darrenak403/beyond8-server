using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Learning.Domain.Entities;

public class SectionProgress : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid SectionId { get; set; }
    public Guid CourseId { get; set; }
    public Guid EnrollmentId { get; set; }

    [ForeignKey(nameof(EnrollmentId))]
    public virtual Enrollment Enrollment { get; set; } = null!;

    public bool AssignmentSubmitted { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? AssignmentGrade { get; set; }

    public DateTime? AssignmentSubmittedAt { get; set; }
    public DateTime? AssignmentGradedAt { get; set; }
}
