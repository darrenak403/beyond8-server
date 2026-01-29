using System.ComponentModel.DataAnnotations;

namespace Beyond8.Catalog.Application.Dtos.Sections;

public class CreateSectionRequest
{
    [Required]
    public Guid CourseId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int OrderIndex { get; set; }
}