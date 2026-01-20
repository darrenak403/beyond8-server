namespace Beyond8.Common.Events.Identity;

public record InstructorUpdateRequestEmailEvent(
    string ToEmail,
    string InstructorName,
    string UpdateNotes,
    DateTime Timestamp
);
