using System.ComponentModel.DataAnnotations;

namespace Beyond8.Catalog.Application.Dtos.Sections;

public class ReorderSectionRequest
{
    [Required]
    public Guid SectionId { get; set; }

    [Required]
    public int NewOrderIndex { get; set; }
}