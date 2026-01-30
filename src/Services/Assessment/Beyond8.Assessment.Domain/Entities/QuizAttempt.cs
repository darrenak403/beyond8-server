using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Assessment.Domain.Enums;
using Beyond8.Common.Data.Base;

namespace Beyond8.Assessment.Domain.Entities;

public class QuizAttempt : BaseEntity
{
    public Guid StudentId { get; set; }

    public Guid QuizId { get; set; }

    [ForeignKey(nameof(QuizId))]
    public virtual Quiz Quiz { get; set; } = null!;

    public int AttemptNumber { get; set; } = 1;

    public DateTime StartedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    /// <summary>
    /// Seed for Fisher-Yates shuffle to reproduce the same order
    /// </summary>
    public int ShuffleSeed { get; set; }


    [Column(TypeName = "jsonb")]
    public string QuestionOrder { get; set; } = "[]";


    [Column(TypeName = "jsonb")]
    public string OptionOrders { get; set; } = "{}";


    [Column(TypeName = "jsonb")]
    public string Answers { get; set; } = "{}";


    [Column(TypeName = "jsonb")]
    public string? QuestionSnapshot { get; set; }


    [Column(TypeName = "jsonb")]
    public string FlaggedQuestions { get; set; } = "[]";


    [Column(TypeName = "decimal(10, 2)")]
    public decimal? Score { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? ScorePercent { get; set; }

    public bool? IsPassed { get; set; }

    public int TimeSpentSeconds { get; set; } = 0;

    // Status
    public QuizAttemptStatus Status { get; set; } = QuizAttemptStatus.InProgress;
}
