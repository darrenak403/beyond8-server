using System.ComponentModel.DataAnnotations;

namespace Beyond8.Catalog.Application.Dtos.Sections;

public class UpdateSectionRequest
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsPublished { get; set; } = true;
}