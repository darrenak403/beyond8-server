namespace Beyond8.Common.Events.Identity;

public record OtpEmailEvent(
    Guid UserId,
    string ToEmail,
    string ToName,
    string OtpCode,
    string Purpose,
    DateTime Timestamp
);
