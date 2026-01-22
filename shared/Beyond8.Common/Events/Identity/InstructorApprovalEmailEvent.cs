using System;

namespace Beyond8.Common.Events.Identity;

public record InstructorApprovalEmailEvent(
    Guid UserId,
    string ToEmail,
    string InstructorName,
    string ProfileUrl,
    DateTime Timestamp
);
