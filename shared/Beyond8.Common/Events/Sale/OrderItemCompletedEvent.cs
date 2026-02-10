namespace Beyond8.Common.Events.Sale;

public record OrderItemCompletedEvent(
    Guid OrderId,
    Guid CourseId,
    string CourseTitle,
    Guid InstructorId,
    string InstructorName,
    decimal LineTotal,
    decimal PlatformFeeAmount,
    decimal InstructorEarnings,
    DateTime PaidAt
);
