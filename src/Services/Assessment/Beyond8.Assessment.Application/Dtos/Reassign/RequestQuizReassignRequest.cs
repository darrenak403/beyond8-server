using Beyond8.Assessment.Domain.Enums;

namespace Beyond8.Assessment.Application.Dtos.Reassign;

public class RequestQuizReassignRequest
{
    public ReassignRequestReason Reason { get; set; }

    public string? Note { get; set; }
}
