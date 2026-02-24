using Beyond8.Common.Utilities;

namespace Beyond8.Assessment.Application.Dtos.Reassign;

public class GetReassignOverviewRequest : PaginationRequest
{
    /// <summary>Tìm theo tên quiz/assignment (SourceTitle) hoặc ghi chú (Note).</summary>
    public string? Search { get; set; }
}
