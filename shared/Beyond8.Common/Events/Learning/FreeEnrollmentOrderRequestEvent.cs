using System;

namespace Beyond8.Common.Events.Learning;

public record FreeEnrollmentOrderRequestEvent(
    Guid UserId,
    Guid CourseId,
    Guid EnrollmentId,
    decimal Amount
);