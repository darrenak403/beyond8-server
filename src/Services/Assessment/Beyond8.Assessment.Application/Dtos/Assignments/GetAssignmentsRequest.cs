using Beyond8.Common.Utilities;

namespace Beyond8.Assessment.Application.Dtos.Assignments;

public class GetAssignmentsRequest : PaginationRequest
{
    public Guid? CourseId { get; set; }
    public Guid? SectionId { get; set; }
}
