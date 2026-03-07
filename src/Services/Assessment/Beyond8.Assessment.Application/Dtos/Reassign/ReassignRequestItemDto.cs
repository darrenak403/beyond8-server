using Beyond8.Assessment.Domain.Enums;

namespace Beyond8.Assessment.Application.Dtos.Reassign;

public class ReassignRequestItemDto
{
    public Guid Id { get; set; }
    public ReassignType Type { get; set; }
    public Guid SourceId { get; set; }
    public string SourceTitle { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public ReassignRequestReason Reason { get; set; }
    public string? Note { get; set; }
    public DateTime RequestedAt { get; set; }
    public ReassignRequestStatus Status { get; set; }
}
