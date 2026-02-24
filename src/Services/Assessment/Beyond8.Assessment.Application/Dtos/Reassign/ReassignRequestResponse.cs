using Beyond8.Assessment.Domain.Enums;

namespace Beyond8.Assessment.Application.Dtos.Reassign;

public class ReassignRequestResponse
{
    public Guid Id { get; set; }
    public ReassignRequestStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
}
