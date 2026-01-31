using System.ComponentModel.DataAnnotations;

namespace Beyond8.Catalog.Application.Dtos.Sections;

public class ChangeAssignmentForSectionRequest
{
    public Guid AssignmentId { get; set; }
}