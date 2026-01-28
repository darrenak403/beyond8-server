using Beyond8.Common.Utilities;

namespace Beyond8.Integration.Application.Dtos.Usages
{
    /// <summary>Search params cho GET /all: pagination + optional date range.</summary>
    public class AiUsageSearchRequest : PaginationRequest
    {
        /// <summary>Lọc từ ngày (optional).</summary>
        public DateTime? StartDate { get; set; }

        /// <summary>Lọc đến ngày (optional).</summary>
        public DateTime? EndDate { get; set; }
    }
}
