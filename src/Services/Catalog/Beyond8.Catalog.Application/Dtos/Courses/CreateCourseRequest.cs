using System.ComponentModel.DataAnnotations;
using Beyond8.Catalog.Domain.Enums;

namespace Beyond8.Catalog.Application.Dtos.Courses;

public class CreateCourseRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public Guid? CategoryId { get; set; }
    public CourseLevel? Level { get; set; }
    public string? Language { get; set; }
    public decimal? Price { get; set; }
    public string? ThumbnailUrl { get; set; }
    public List<string>? Outcomes { get; set; }
    public List<string>? Requirements { get; set; }
    public List<string>? TargetAudience { get; set; }
}