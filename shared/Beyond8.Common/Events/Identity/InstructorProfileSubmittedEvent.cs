namespace Beyond8.Common.Events.Identity;

public record InstructorProfileSubmittedEvent(
    Guid UserId,
    Guid ProfileId,
    string InstructorName,
    string Email,
    DateTime Timestamp
);
