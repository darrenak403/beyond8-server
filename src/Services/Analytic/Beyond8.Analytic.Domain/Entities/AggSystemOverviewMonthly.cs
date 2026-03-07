using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Analytic.Domain.Entities;

/// <summary>
/// Dữ liệu thống kê hệ thống theo tháng (incremental, không lũy kế).
/// Mỗi record = 1 tháng. Dùng để vẽ chart và tính % thay đổi so với tháng trước.
/// </summary>
public class AggSystemOverviewMonthly : BaseEntity
{
    [MaxLength(7), Required]
    public string YearMonth { get; set; } = string.Empty; // "2026-03"

    public int Year { get; set; }
    public int Month { get; set; }

    // Incremental user counts (registered this month)
    public int NewUsers { get; set; } = 0;
    public int NewStudents { get; set; } = 0;
    public int NewInstructors { get; set; } = 0;

    // Incremental course & enrollment counts
    public int NewCourses { get; set; } = 0;
    public int NewPublishedCourses { get; set; } = 0;
    public int NewEnrollments { get; set; } = 0;
    public int NewCompletedEnrollments { get; set; } = 0;

    // Incremental revenue this month
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Revenue { get; set; } = 0;

    /// <summary>Platform fee = platform profit this month.</summary>
    [Column(TypeName = "decimal(18, 2)")]
    public decimal PlatformProfit { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal InstructorEarnings { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal RefundAmount { get; set; } = 0;
}
