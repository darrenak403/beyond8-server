using Beyond8.Common.Utilities;

namespace Beyond8.Integration.Application.Dtos.Usages
{
    public class AiUsageSearchRequest : PaginationRequest
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
