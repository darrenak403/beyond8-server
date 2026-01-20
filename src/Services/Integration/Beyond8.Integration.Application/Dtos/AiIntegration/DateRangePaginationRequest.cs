using Beyond8.Common.Utilities;

namespace Beyond8.Integration.Application.Dtos.AiIntegration;

public class DateRangePaginationRequest : PaginationRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
