namespace Beyond8.Assessment.Application.Dtos.AssignmentSubmissions;

public class UpdateSubmissionRequest
{
    public string? TextContent { get; set; }

    public List<string>? FileUrls { get; set; }
}
