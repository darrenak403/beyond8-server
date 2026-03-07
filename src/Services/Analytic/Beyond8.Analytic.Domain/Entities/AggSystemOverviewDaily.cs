using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Analytic.Domain.Entities;

/// <summary>
/// Dữ liệu thống kê hệ thống theo ngày (incremental, không lũy kế).
/// Mỗi record = 1 ngày. Dùng để vẽ chart cho groupBy=Month và groupBy=Custom.
/// </summary>
public class AggSystemOverviewDaily : BaseEntity
{
    [MaxLength(10), Required]
    public string DateKey { get; set; } = string.Empty; // "2026-03-15"

    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }

    // Incremental enrollment counts this day
    public int NewEnrollments { get; set; } = 0;
    public int NewCompletedEnrollments { get; set; } = 0;

    // Incremental revenue this day
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Revenue { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PlatformProfit { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal InstructorEarnings { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal RefundAmount { get; set; } = 0;
}
