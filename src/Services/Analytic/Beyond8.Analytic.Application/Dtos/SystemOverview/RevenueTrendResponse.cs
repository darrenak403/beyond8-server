namespace Beyond8.Analytic.Application.Dtos.SystemOverview;

public class RevenueTrendResponse
{
    /// <summary>Human-readable period label, e.g. "Năm 2026", "Q1/2026", "Tháng 3/2026"</summary>
    public string PeriodLabel { get; set; } = string.Empty;

    public decimal TotalRevenue { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal TotalInstructorEarnings { get; set; }
    public int TotalNewEnrollments { get; set; }

    public List<RevenueDataPoint> DataPoints { get; set; } = [];
}

public class RevenueDataPoint
{
    /// <summary>ISO key: "2026-03" (monthly) or "2026-03-15" (daily)</summary>
    public string Period { get; set; } = string.Empty;

    /// <summary>Display label: "Tháng 3" or "15/3"</summary>
    public string Label { get; set; } = string.Empty;

    public decimal Revenue { get; set; }
    public decimal Profit { get; set; }
    public decimal InstructorEarnings { get; set; }
    public int NewEnrollments { get; set; }
}
