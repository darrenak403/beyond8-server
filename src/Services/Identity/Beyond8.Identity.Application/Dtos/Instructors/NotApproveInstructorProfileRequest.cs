using Beyond8.Identity.Domain.Enums;

namespace Beyond8.Identity.Application.Dtos.Instructors;

public class NotApproveInstructorProfileRequest
{
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.RequestUpdate;
    public string NotApproveReason { get; set; } = string.Empty;
}