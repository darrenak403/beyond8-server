using Beyond8.Analytic.Domain.Enums;

namespace Beyond8.Analytic.Application.Dtos.SystemOverview;

public class RevenueTrendRequest
{
    public GroupByPeriod GroupBy { get; set; } = GroupByPeriod.Year;

    /// <summary>Required for GroupBy = Year, Quarter, Month</summary>
    public int? Year { get; set; }

    /// <summary>1–4. Required for GroupBy = Quarter</summary>
    public int? Quarter { get; set; }

    /// <summary>1–12. Required for GroupBy = Month</summary>
    public int? Month { get; set; }

    /// <summary>Required for GroupBy = Custom</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>Required for GroupBy = Custom</summary>
    public DateTime? EndDate { get; set; }
}
