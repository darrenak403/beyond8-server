namespace Beyond8.Common.Events.Identity
{
    public record UserUpdatedEvent
    (
        Guid UserId,
        string? FullName,
        string? Email
    );
}