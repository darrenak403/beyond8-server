namespace Beyond8.Common.Events.Identity;

public record InstructorRejectionEmailEvent(
    string ToEmail,
    string InstructorName,
    string Reason,
    DateTime Timestamp
);
