using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Analytic.Domain.Entities;

public class AggSystemOverview : BaseEntity
{
    // Users
    public int TotalUsers { get; set; } = 0;
    public int TotalActiveUsers { get; set; } = 0;
    public int NewUsersToday { get; set; } = 0;
    public int TotalInstructors { get; set; } = 0;
    public int TotalStudents { get; set; } = 0;

    // Courses
    public int TotalCourses { get; set; } = 0;
    public int TotalPublishedCourses { get; set; } = 0;

    public int TotalEnrollments { get; set; } = 0;
    public int TotalCompletedEnrollments { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalRevenue { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalPlatformFee { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalInstructorEarnings { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalRefundAmount { get; set; } = 0;

    [Column(TypeName = "decimal(5, 2)")]
    public decimal AvgCourseCompletionRate { get; set; } = 0;

    [Column(TypeName = "decimal(3, 2)")]
    public decimal AvgCourseRating { get; set; } = 0;

    public int TotalReviews { get; set; } = 0;

    public DateOnly SnapshotDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public bool IsCurrent { get; set; } = true;
}
