using System.ComponentModel.DataAnnotations;
using Beyond8.Catalog.Domain.Enums;

namespace Beyond8.Catalog.Application.Dtos.Courses;

public class CreateCourseRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public Guid CategoryId { get; set; }
    public CourseLevel Level { get; set; } = CourseLevel.Beginner;
    public string Language { get; set; } = "vi-VN";
    public decimal Price { get; set; } = 0;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public List<string> Outcomes { get; set; } = [];
    public List<string>? Requirements { get; set; }
    public List<string>? TargetAudience { get; set; }
    // Set by API layer from current user
    public Guid InstructorId { get; set; }
}