namespace Beyond8.Common.Events.Identity;

public record InstructorUpdateRequestEvent(
    Guid UserId,
    string ToEmail,
    string InstructorName,
    string UpdateNotes,
    DateTime Timestamp
);
