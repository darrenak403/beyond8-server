using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Learning.Domain.Entities;

public class CourseCertificateEligibilityConfig : BaseEntity
{
    public Guid CourseId { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? QuizAverageMinPercent { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? AssignmentAverageMinPercent { get; set; }
}
