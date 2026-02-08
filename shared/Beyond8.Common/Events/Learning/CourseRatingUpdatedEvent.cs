namespace Beyond8.Common.Events.Learning;

public record CourseRatingUpdatedEvent(
    Guid CourseId,
    Guid InstructorId,
    decimal CourseAvgRating,
    int CourseTotalReviews,
    decimal InstructorAvgRating,
    DateTime Timestamp
);
