using Beyond8.Learning.Application.Dtos.Catalog;
using Beyond8.Learning.Application.Dtos.Enrollments;
using Beyond8.Learning.Domain.Entities;
using Beyond8.Learning.Domain.Enums;

namespace Beyond8.Learning.Application.Mappings;

public static class EnrollmentMappings
{
    public static Enrollment ToEnrollmentEntity(
    this CourseStructureResponse structure,
    Guid userId,
    Guid courseId,
    int totalLessons,
    decimal pricePaid = 0)
    {
        return new Enrollment
        {
            UserId = userId,
            CourseId = courseId,
            CourseTitle = structure.Title,
            CourseThumbnailUrl = structure.ThumbnailUrl,
            Slug = structure.Slug,
            InstructorId = structure.InstructorId,
            InstructorName = structure.InstructorName,
            PricePaid = pricePaid,
            Status = EnrollmentStatus.Active,
            ProgressPercent = 0,
            CompletedLessons = 0,
            TotalLessons = totalLessons,
            EnrolledAt = DateTime.UtcNow
        };
    }

    public static SectionProgress ToSectionProgressEntity(
        this SectionStructureItem section,
        Guid userId,
        Guid courseId,
        Guid enrollmentId)
    {
        return new SectionProgress
        {
            UserId = userId,
            SectionId = section.Id,
            CourseId = courseId,
            EnrollmentId = enrollmentId,
            AssignmentSubmitted = false
        };
    }

    public static LessonProgress ToLessonProgressEntity(
        this LessonStructureItem lesson,
        Guid userId,
        Guid courseId,
        Guid enrollmentId)
    {
        return new LessonProgress
        {
            UserId = userId,
            LessonId = lesson.Id,
            CourseId = courseId,
            EnrollmentId = enrollmentId,
            TotalDurationSeconds = lesson.DurationSeconds ?? 0
        };
    }

    public static EnrollmentResponse ToResponse(this Enrollment entity)
    {
        return new EnrollmentResponse
        {
            Id = entity.Id,
            UserId = entity.UserId,
            CourseId = entity.CourseId,
            CourseTitle = entity.CourseTitle,
            CourseThumbnailUrl = entity.CourseThumbnailUrl,
            Slug = entity.Slug,
            InstructorId = entity.InstructorId,
            InstructorName = entity.InstructorName,
            PricePaid = entity.PricePaid,
            Status = entity.Status,
            ProgressPercent = entity.ProgressPercent,
            CompletedLessons = entity.CompletedLessons,
            TotalLessons = entity.TotalLessons,
            EnrolledAt = entity.EnrolledAt,
            CompletedAt = entity.CompletedAt,
            LastAccessedAt = entity.LastAccessedAt,
            LastAccessedLessonId = entity.LastAccessedLessonId
        };
    }

    public static EnrollmentSimpleResponse ToSimpleResponse(this Enrollment entity, decimal? progressPercent = null)
    {
        return new EnrollmentSimpleResponse
        {
            Id = entity.Id,
            CourseId = entity.CourseId,
            CourseTitle = entity.CourseTitle,
            CourseThumbnailUrl = entity.CourseThumbnailUrl,
            Slug = entity.Slug,
            InstructorId = entity.InstructorId,
            InstructorName = entity.InstructorName,
            ProgressPercent = progressPercent ?? entity.ProgressPercent
        };
    }
}
