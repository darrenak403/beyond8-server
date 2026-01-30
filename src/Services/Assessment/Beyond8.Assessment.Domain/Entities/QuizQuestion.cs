using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Assessment.Domain.Entities;

/// <summary>
/// Junction table linking Quiz and Question with ordering
/// </summary>
public class QuizQuestion : BaseEntity
{
    public Guid QuizId { get; set; }

    [ForeignKey(nameof(QuizId))]
    public virtual Quiz Quiz { get; set; } = null!;

    public Guid QuestionId { get; set; }

    [ForeignKey(nameof(QuestionId))]
    public virtual Question Question { get; set; } = null!;

    /// <summary>
    /// Order of question in the quiz (1-based)
    /// </summary>
    public int OrderIndex { get; set; }

    /// <summary>
    /// Override points for this question in this quiz (null = use question's default)
    /// </summary>
    public int? PointsOverride { get; set; }
}
