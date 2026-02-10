using Beyond8.Common.Utilities;

namespace Beyond8.Analytic.Application.Dtos.Common;

public class DateRangeAnalyticRequest : PaginationRequest
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}
