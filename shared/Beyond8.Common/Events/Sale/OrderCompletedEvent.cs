namespace Beyond8.Common.Events.Sale;

public record OrderCompletedEvent(
    Guid OrderId,
    Guid UserId,
    List<Guid> CourseIds
);
