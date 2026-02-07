namespace Beyond8.Common.Events.Identity;

public record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string RoleCode,
    DateTime RegisteredAt
);
