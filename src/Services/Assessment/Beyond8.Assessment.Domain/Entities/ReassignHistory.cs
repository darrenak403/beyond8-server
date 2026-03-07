using Beyond8.Assessment.Domain.Enums;
using Beyond8.Common.Data.Base;

namespace Beyond8.Assessment.Domain.Entities;

public class ReassignHistory : BaseEntity
{
    public ReassignType Type { get; set; }

    public Guid SourceId { get; set; }

    public Guid StudentId { get; set; }

    public Guid ResetByInstructorId { get; set; }

    public Guid? LessonId { get; set; }

    public Guid? SectionId { get; set; }

    public DateTime ResetAt { get; set; }

    public int DeletedCount { get; set; }

    public Guid? ReassignRequestId { get; set; }
}
