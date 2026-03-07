using System.Text.Json.Serialization;

namespace Beyond8.Learning.Application.Dtos.Catalog;

public class CourseStructureResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal FinalPrice { get; set; }
    public string? ThumbnailUrl { get; set; }
    public Guid InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public int TotalLessons { get; set; }

    [JsonConverter(typeof(CourseStatusIntConverter))]
    public int Status { get; set; }
    public List<SectionStructureItem> Sections { get; set; } = [];
}
