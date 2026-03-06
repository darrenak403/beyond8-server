namespace Beyond8.Analytic.Application.Dtos.SystemOverview;

public class SystemDashboardResponse
{
    // KPI cards
    public int TotalUsers { get; set; }
    public int TotalInstructors { get; set; }
    public int TotalStudents { get; set; }
    public int TotalCourses { get; set; }
    public int TotalPublishedCourses { get; set; }
    public int TotalEnrollments { get; set; }
    public int TotalCompletedEnrollments { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalPlatformFee { get; set; }
    public decimal TotalInstructorEarnings { get; set; }
    public decimal AvgCourseRating { get; set; }

    // Current year summary
    public decimal CurrentYearRevenue { get; set; }
    public decimal CurrentYearProfit { get; set; }

    // 12-month bar chart — Phân tích doanh thu
    public List<MonthlyDataPoint> RevenueTrend12M { get; set; } = [];

    // 6-month horizontal chart — Dòng tiền & Lợi nhuận ròng
    public List<MonthlyDataPoint> CashflowTrend6M { get; set; } = [];

    public DateTime? UpdatedAt { get; set; }
}

public class MonthlyDataPoint
{
    /// <summary>ISO format: "2026-03"</summary>
    public string YearMonth { get; set; } = string.Empty;
    /// <summary>Display label: "Tháng 3" or "T3"</summary>
    public string Label { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal Profit { get; set; }
}
