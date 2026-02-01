using Beyond8.Catalog.Application.Dtos.Sections;
using Beyond8.Catalog.Application.Mappings.LessonMappings;
using Beyond8.Catalog.Domain.Entities;

namespace Beyond8.Catalog.Application.Mappings.SectionMappings;

public static class SectionMappingExtensions
{
    public static SectionResponse ToResponse(this Section section)
    {
        return new SectionResponse
        {
            Id = section.Id,
            CourseId = section.CourseId,
            Title = section.Title,
            Description = section.Description,
            OrderIndex = section.OrderIndex,
            IsPublished = section.IsPublished,
            TotalLessons = section.TotalLessons,
            TotalDurationMinutes = section.TotalDurationMinutes,
            AssignmentId = section.AssignmentId,
            CreatedAt = section.CreatedAt,
            UpdatedAt = section.UpdatedAt
        };
    }

    public static Section ToEntity(this CreateSectionRequest request, int orderIndex)
    {
        return new Section
        {
            CourseId = request.CourseId,
            Title = request.Title,
            Description = request.Description,
            IsPublished = true, // Default value
            AssignmentId = request.AssignmentId,
            OrderIndex = orderIndex
        };
    }

    public static void UpdateFrom(this Section section, UpdateSectionRequest request)
    {
        section.Title = request.Title;
        section.Description = request.Description;
        section.IsPublished = request.IsPublished;
        section.AssignmentId = request.AssignmentId;
    }

    public static SectionSummaryResponse ToSummaryResponse(this Section section)
    {
        return new SectionSummaryResponse
        {
            Id = section.Id,
            Title = section.Title,
            Description = section.Description,
            Order = section.OrderIndex,
            TotalLessons = section.Lessons?.Count ?? 0,
            TotalDurationMinutes = section.Lessons?.Sum(l => (l.Video?.DurationSeconds ?? 0) / 60) ?? 0,
            Lessons = section.Lessons?
                .OrderBy(l => l.OrderIndex)
                .Select(l => l.ToSummaryResponse())
                .ToList() ?? []
        };
    }

    public static SectionDetailResponse ToDetailResponse(this Section section)
    {
        return new SectionDetailResponse
        {
            Id = section.Id,
            CourseId = section.CourseId,
            Title = section.Title,
            Description = section.Description,
            OrderIndex = section.OrderIndex,
            IsPublished = section.IsPublished,
            TotalLessons = section.Lessons?.Count ?? 0,
            TotalDurationMinutes = section.Lessons?.Sum(l => (l.Video?.DurationSeconds ?? 0) / 60) ?? 0,
            AssignmentId = section.AssignmentId,
            CreatedAt = section.CreatedAt,
            UpdatedAt = section.UpdatedAt,
            Lessons = section.Lessons?
                .OrderBy(l => l.OrderIndex)
                .Select(l => l.ToDetailResponse())
                .ToList() ?? []
        };
    }
}