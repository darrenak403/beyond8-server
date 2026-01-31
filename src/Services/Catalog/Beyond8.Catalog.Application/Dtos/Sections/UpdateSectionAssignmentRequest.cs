using System.ComponentModel.DataAnnotations;

namespace Beyond8.Catalog.Application.Dtos.Sections;

public class UpdateSectionAssignmentRequest
{
    [Required]
    public Guid AssignmentId { get; set; }
}