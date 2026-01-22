using System;

namespace Beyond8.Common.Events.Identity;

public record InstructorRejectionEmailEvent(
    Guid UserId,
    string ToEmail,
    string InstructorName,
    string Reason,
    DateTime Timestamp
);
