using Beyond8.Assessment.Domain.Enums;

namespace Beyond8.Assessment.Application.Dtos.AssignmentSubmissions;

public class AssignmentSubmissionDetail
{
    public Guid AssignmentId { get; set; }
    public string AssignmentTitle { get; set; } = string.Empty;
    public int TotalSubmissions { get; set; }
    public int UngradedSubmissions { get; set; }
    public List<SubmissionResponse> Submissions { get; set; } = [];
}