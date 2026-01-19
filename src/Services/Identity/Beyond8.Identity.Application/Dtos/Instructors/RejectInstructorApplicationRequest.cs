using Beyond8.Identity.Domain.Enums;

namespace Beyond8.Identity.Application.Dtos.Instructors;

public class RejectInstructorApplicationRequest
{
    public string RejectionReason { get; set; } = string.Empty;
}