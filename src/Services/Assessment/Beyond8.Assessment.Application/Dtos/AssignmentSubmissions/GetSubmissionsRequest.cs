using Beyond8.Assessment.Domain.Enums;
using Beyond8.Common.Utilities;

namespace Beyond8.Assessment.Application.Dtos.AssignmentSubmissions;

public class GetSubmissionsRequest : PaginationRequest
{
    public Guid? AssignmentId { get; set; }
    public Guid? StudentId { get; set; }
    public SubmissionStatus? Status { get; set; }
}
