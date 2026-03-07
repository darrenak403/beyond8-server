namespace Beyond8.Common.Events.Learning;

public record CourseCompletedEvent(
    Guid EnrollmentId,
    Guid UserId,
    Guid CourseId,
    string CourseTitle,
    DateTime CompletedAt,
    Guid? CertificateId = null,
    string? UserEmail = null,
    string? UserFullName = null
);
