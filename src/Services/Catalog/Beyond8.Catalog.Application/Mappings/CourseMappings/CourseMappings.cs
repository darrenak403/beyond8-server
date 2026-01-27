using Beyond8.Catalog.Application.Dtos.Courses;
using Beyond8.Catalog.Domain.Entities;
using Beyond8.Catalog.Domain.Enums;
using Beyond8.Common.Utilities;
using System.Text.Json;

namespace Beyond8.Catalog.Application.Mappings.CourseMappings;

public static class CourseMappings
{
    public static Course ToEntity(this CreateCourseRequest request)
    {
        var slug = request.Title.ToSlug();
        var course = new Course
        {
            Title = request.Title,
            Slug = slug,
            Description = request.Description,
            ShortDescription = request.ShortDescription,
            CategoryId = request.CategoryId,
            InstructorId = request.InstructorId,
            Level = request.Level,
            Language = request.Language,
            Price = request.Price,
            ThumbnailUrl = request.ThumbnailUrl,
            Outcomes = JsonSerializer.Serialize(request.Outcomes),
            Requirements = request.Requirements != null ? JsonSerializer.Serialize(request.Requirements) : null,
            TargetAudience = request.TargetAudience != null ? JsonSerializer.Serialize(request.TargetAudience) : null,
            Status = CourseStatus.Draft,
            IsActive = true
        };

        return course;
    }

    public static void UpdateMetadataFromRequest(this Course entity, UpdateCourseMetadataRequest request)
    {
        entity.Title = request.Title;
        entity.Slug = request.Title.ToSlug();
        entity.ShortDescription = request.ShortDescription;
        entity.CategoryId = request.CategoryId;
        entity.Level = request.Level;
        entity.Language = request.Language;
        entity.Price = request.Price;
        entity.ThumbnailUrl = request.ThumbnailUrl;
        entity.Outcomes = JsonSerializer.Serialize(request.Outcomes);
        entity.Requirements = request.Requirements != null ? JsonSerializer.Serialize(request.Requirements) : null;
        entity.TargetAudience = request.TargetAudience != null ? JsonSerializer.Serialize(request.TargetAudience) : null;
    }

    public static void UpdateContentFromRequest(this Course entity, UpdateCourseContentRequest request)
    {
        entity.Description = request.Description;
        // Future: Update sections, lessons, etc.
    }

    public static CourseResponse ToResponse(this Course entity)
    {
        return new CourseResponse
        {
            Id = entity.Id,
            Title = entity.Title,
            Slug = entity.Slug,
            ShortDescription = entity.ShortDescription,
            CategoryId = entity.CategoryId,
            CategoryName = entity.Category?.Name ?? string.Empty,
            InstructorId = entity.InstructorId,
            InstructorName = string.Empty, // TODO: Get from Identity service
            Status = entity.Status,
            Level = entity.Level,
            Language = entity.Language,
            Price = entity.Price,
            ThumbnailUrl = entity.ThumbnailUrl,
            TotalStudents = entity.TotalStudents,
            TotalSections = entity.Sections?.Count ?? 0,
            TotalLessons = entity.Sections?.Sum(s => s.Lessons.Count) ?? 0,
            TotalDurationMinutes = entity.Sections?.Sum(s => s.Lessons.Sum(l => (l.DurationSeconds ?? 0) / 60)) ?? 0,
            AvgRating = entity.AvgRating,
            TotalReviews = entity.TotalReviews,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}