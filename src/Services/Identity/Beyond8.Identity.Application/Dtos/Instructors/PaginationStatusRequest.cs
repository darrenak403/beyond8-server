using Beyond8.Common.Utilities;

namespace Beyond8.Identity.Application.Dtos.Instructors;

public class PaginationStatusRequest : PaginationRequest
{
    public VerificationStatusRequest Status { get; set; } = VerificationStatusRequest.All;
}

public enum VerificationStatusRequest
{
    All = 0,
    Pending = 1,
    Verified = 2,
    RequestUpdate = 3
}
