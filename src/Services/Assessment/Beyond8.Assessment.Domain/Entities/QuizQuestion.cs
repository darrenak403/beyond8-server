using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Assessment.Domain.Entities;

public class QuizQuestion : BaseEntity
{
    public Guid QuizId { get; set; }

    [ForeignKey(nameof(QuizId))]
    public virtual Quiz Quiz { get; set; } = null!;

    public Guid QuestionId { get; set; }

    [ForeignKey(nameof(QuestionId))]
    public virtual Question Question { get; set; } = null!;

    public int OrderIndex { get; set; }
    
    public int? PointsOverride { get; set; }
}
