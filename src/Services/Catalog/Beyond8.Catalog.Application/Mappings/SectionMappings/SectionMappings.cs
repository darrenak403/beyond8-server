using Beyond8.Catalog.Application.Dtos.Sections;
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

    public static Section ToEntity(this CreateSectionRequest request)
    {
        return new Section
        {
            CourseId = request.CourseId,
            Title = request.Title,
            Description = request.Description,
            OrderIndex = request.OrderIndex,
            IsPublished = true, // Default value
            AssignmentId = request.AssignmentId
        };
    }

    public static void UpdateFrom(this Section section, UpdateSectionRequest request)
    {
        section.Title = request.Title;
        section.Description = request.Description;
        section.IsPublished = request.IsPublished;
        section.AssignmentId = request.AssignmentId;
    }
}