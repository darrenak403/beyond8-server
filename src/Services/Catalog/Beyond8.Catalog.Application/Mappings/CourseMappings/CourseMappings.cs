using Beyond8.Catalog.Application.Dtos.Courses;
using Beyond8.Catalog.Application.Mappings.SectionMappings;
using Beyond8.Catalog.Domain.Entities;
using Beyond8.Catalog.Domain.Enums;
using System.Text.Json;
using Beyond8.Catalog.Application.Helpers;

namespace Beyond8.Catalog.Application.Mappings.CourseMappings;

public static class CourseMappings
{
    public static Course ToEntity(this CreateCourseRequest request, Guid instructorId, string instructorName)
    {
        var slug = request.Title.ToSlug();
        var course = new Course
        {
            Title = request.Title,
            Slug = slug,
            Description = request.Description ?? string.Empty,
            ShortDescription = request.ShortDescription,
            CategoryId = request.CategoryId ?? Guid.Empty, // Or handle null category
            Level = request.Level ?? CourseLevel.Beginner,
            Language = request.Language ?? "vi-VN",
            Price = request.Price ?? 0,
            ThumbnailUrl = request.ThumbnailUrl ?? string.Empty,
            Outcomes = request.Outcomes != null ? JsonSerializer.Serialize(request.Outcomes) : "[]",
            Requirements = request.Requirements != null ? JsonSerializer.Serialize(request.Requirements) : null,
            TargetAudience = request.TargetAudience != null ? JsonSerializer.Serialize(request.TargetAudience) : null,
            Status = CourseStatus.Draft,
            IsActive = true,
            InstructorId = instructorId,
            InstructorName = instructorName
        };

        return course;
    }

    public static void UpdateMetadataFromRequest(this Course entity, UpdateCourseMetadataRequest request)
    {
        entity.Title = request.Title;
        entity.Slug = request.Title.ToSlug();
        entity.Description = request.Description ?? string.Empty;
        entity.ShortDescription = request.ShortDescription;
        entity.CategoryId = request.CategoryId ?? entity.CategoryId; // Keep existing if null
        entity.Level = request.Level ?? entity.Level;
        entity.Language = request.Language ?? entity.Language;
        entity.Price = request.Price ?? entity.Price;
        entity.ThumbnailUrl = request.ThumbnailUrl ?? entity.ThumbnailUrl;
        entity.Outcomes = request.Outcomes != null ? JsonSerializer.Serialize(request.Outcomes) : entity.Outcomes;
        entity.Requirements = request.Requirements != null ? JsonSerializer.Serialize(request.Requirements) : entity.Requirements;
        entity.TargetAudience = request.TargetAudience != null ? JsonSerializer.Serialize(request.TargetAudience) : entity.TargetAudience;
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
            InstructorName = entity.InstructorName,
            Status = entity.Status,
            Level = entity.Level,
            Language = entity.Language,
            Price = entity.Price,
            ThumbnailUrl = entity.ThumbnailUrl,
            TotalStudents = entity.TotalStudents,
            TotalSections = entity.Sections?.Count ?? 0,
            TotalLessons = entity.Sections?.Sum(s => s.Lessons.Count) ?? 0,
            TotalDurationMinutes = entity.Sections?.Sum(s => s.Lessons.Sum(l => (l.Video?.DurationSeconds ?? 0) / 60)) ?? 0,
            AvgRating = entity.AvgRating,
            TotalReviews = entity.TotalReviews,
            Outcomes = JsonSerializer.Deserialize<List<string>>(entity.Outcomes) ?? [],
            Requirements = !string.IsNullOrEmpty(entity.Requirements)
                ? JsonSerializer.Deserialize<List<string>>(entity.Requirements)
                : null,
            TargetAudience = !string.IsNullOrEmpty(entity.TargetAudience)
                ? JsonSerializer.Deserialize<List<string>>(entity.TargetAudience)
                : null,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public static CourseSummaryResponse ToSummaryResponse(this Course entity)
    {
        return new CourseSummaryResponse
        {
            Id = entity.Id,
            Title = entity.Title,
            Slug = entity.Slug,
            ShortDescription = entity.ShortDescription,
            CategoryId = entity.CategoryId,
            CategoryName = entity.Category?.Name ?? string.Empty,
            InstructorId = entity.InstructorId,
            InstructorName = entity.InstructorName,
            Status = entity.Status,
            Level = entity.Level,
            Language = entity.Language,
            Price = entity.Price,
            ThumbnailUrl = entity.ThumbnailUrl,
            TotalStudents = entity.TotalStudents,
            TotalSections = entity.Sections?.Count ?? 0,
            TotalLessons = entity.Sections?.Sum(s => s.Lessons.Count) ?? 0,
            TotalDurationMinutes = entity.Sections?.Sum(s => s.Lessons.Sum(l => (l.Video?.DurationSeconds ?? 0) / 60)) ?? 0,
            AvgRating = entity.AvgRating,
            TotalReviews = entity.TotalReviews,
            Outcomes = JsonSerializer.Deserialize<List<string>>(entity.Outcomes) ?? [],
            Requirements = !string.IsNullOrEmpty(entity.Requirements)
                ? JsonSerializer.Deserialize<List<string>>(entity.Requirements)
                : null,
            TargetAudience = !string.IsNullOrEmpty(entity.TargetAudience)
                ? JsonSerializer.Deserialize<List<string>>(entity.TargetAudience)
                : null,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Sections = entity.Sections?
                .OrderBy(s => s.OrderIndex)
                .Select(s => s.ToSummaryResponse())
                .ToList() ?? []
        };
    }

    public static CourseDetailResponse ToDetailResponse(this Course entity)
    {
        return new CourseDetailResponse
        {
            Id = entity.Id,
            Title = entity.Title,
            Slug = entity.Slug,
            ShortDescription = entity.ShortDescription,
            CategoryId = entity.CategoryId,
            CategoryName = entity.Category?.Name ?? string.Empty,
            InstructorId = entity.InstructorId,
            InstructorName = entity.InstructorName,
            Status = entity.Status,
            Level = entity.Level,
            Language = entity.Language,
            Price = entity.Price,
            ThumbnailUrl = entity.ThumbnailUrl,
            TotalStudents = entity.TotalStudents,
            TotalSections = entity.Sections?.Count ?? 0,
            TotalLessons = entity.Sections?.Sum(s => s.Lessons.Count) ?? 0,
            TotalDurationMinutes = entity.Sections?.Sum(s => s.Lessons.Sum(l => (l.Video?.DurationSeconds ?? 0) / 60)) ?? 0,
            AvgRating = entity.AvgRating,
            TotalReviews = entity.TotalReviews,
            Outcomes = JsonSerializer.Deserialize<List<string>>(entity.Outcomes) ?? [],
            Requirements = !string.IsNullOrEmpty(entity.Requirements)
                ? JsonSerializer.Deserialize<List<string>>(entity.Requirements)
                : null,
            TargetAudience = !string.IsNullOrEmpty(entity.TargetAudience)
                ? JsonSerializer.Deserialize<List<string>>(entity.TargetAudience)
                : null,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Sections = entity.Sections?
                .OrderBy(s => s.OrderIndex)
                .Select(s => s.ToDetailResponse())
                .ToList() ?? []
        };
    }

    public static CourseSimpleResponse ToSimpleResponse(this Course entity)
    {
        return new CourseSimpleResponse
        {
            Id = entity.Id,
            Title = entity.Title,
            Slug = entity.Slug,
            ShortDescription = entity.ShortDescription ?? string.Empty,
            ThumbnailUrl = entity.ThumbnailUrl,
            CategoryName = entity.Category?.Name ?? string.Empty,
            Level = entity.Level,
            TotalStudents = entity.TotalStudents,
            TotalDurationMinutes = entity.Sections?.Sum(s => s.Lessons.Sum(l => (l.Video?.DurationSeconds ?? 0) / 60)) ?? 0,
            InstructorId = entity.InstructorId,
            InstructorName = entity.InstructorName,
            Price = entity.Price,
            AvgRating = entity.AvgRating,
            TotalReviews = entity.TotalReviews
        };
    }
}