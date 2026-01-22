using System;

namespace Beyond8.Common.Events.Identity;

public record InstructorApplicationSubmittedEvent(
    Guid UserId,
    Guid ProfileId,
    string InstructorName,
    string Email,
    DateTime Timestamp
);
