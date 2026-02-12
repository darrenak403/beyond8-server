using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Analytic.Domain.Entities;

public class AggInstructorRevenue : BaseEntity
{
    public Guid InstructorId { get; set; }

    [MaxLength(200)]
    public string InstructorName { get; set; } = string.Empty;

    public int TotalCourses { get; set; } = 0;
    public int TotalStudents { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalRevenue { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalPlatformFee { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalInstructorEarnings { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalRefundAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalPaidOut { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PendingBalance { get; set; } = 0;

    [Column(TypeName = "decimal(5, 2)")]
    public decimal AvgCourseRating { get; set; } = 0;

    public int TotalReviews { get; set; } = 0;

    public DateOnly SnapshotDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public bool IsCurrent { get; set; } = true;
}
