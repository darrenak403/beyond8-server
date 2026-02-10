using Beyond8.Assessment.Application.Dtos.AssignmentSubmissions;

namespace Beyond8.Assessment.Application.Dtos.AssignmentSubmissions;

public class CourseSubmissionSummaryResponse
{
    public List<SectionSubmissionSummary> Sections { get; set; } = [];
    public int TotalUngradedSections { get; set; }
    public int TotalAssignments { get; set; }
}