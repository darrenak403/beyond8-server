using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Catalog.Domain.Entities;

public class Section : BaseEntity
{
    public Guid CourseId { get; set; }
    [ForeignKey(nameof(CourseId))]
    public virtual Course Course { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int OrderIndex { get; set; }

    public bool IsPublished { get; set; } = true;

    // Statistics (denormalized)
    public int TotalLessons { get; set; } = 0;
    public int TotalDurationMinutes { get; set; } = 0;

    // Section Assignment
    public Guid? AssignmentId { get; set; }

    // Relationships
    public virtual ICollection<Lesson> Lessons { get; set; } = [];
}
