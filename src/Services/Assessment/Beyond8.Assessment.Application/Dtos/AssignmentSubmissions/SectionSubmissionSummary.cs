namespace Beyond8.Assessment.Application.Dtos.AssignmentSubmissions;

public class SectionSubmissionSummary
{
    public Guid SectionId { get; set; }
    public int TotalSubmissions { get; set; }
    public int UngradedSubmissions { get; set; }
}