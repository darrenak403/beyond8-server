namespace Beyond8.Analytic.Application.Dtos.AiUsage;

public class AiUsageChartRequest
{
    public int? PeriodMonths { get; set; }

    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}
