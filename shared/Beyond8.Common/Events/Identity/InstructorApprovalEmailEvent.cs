namespace Beyond8.Common.Events.Identity;

public record InstructorApprovalEmailEvent(
    string ToEmail,
    string InstructorName,
    string ProfileUrl,
    DateTime Timestamp
);
