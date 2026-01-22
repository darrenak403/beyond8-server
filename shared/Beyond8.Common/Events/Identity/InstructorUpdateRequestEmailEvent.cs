using System;

namespace Beyond8.Common.Events.Identity;

public record InstructorUpdateRequestEmailEvent(
    Guid UserId,
    string ToEmail,
    string InstructorName,
    string UpdateNotes,
    DateTime Timestamp
);
