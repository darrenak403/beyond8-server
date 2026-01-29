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
            CreatedAt = section.CreatedAt,
            UpdatedAt = section.UpdatedAt
        };
    }
}