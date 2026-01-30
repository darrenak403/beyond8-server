using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Assessment.Domain.Entities;

public class Quiz : BaseEntity
{
    public Guid InstructorId { get; set; }

    public Guid? CourseId { get; set; }

    public Guid? LessonId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int? TimeLimitMinutes { get; set; }

    public int PassScorePercent { get; set; } = 60;

    public int TotalPoints { get; set; } = 100;

    public int MaxAttempts { get; set; } = 1;

    public bool ShuffleQuestions { get; set; } = true;

    public bool AllowReview { get; set; } = true;

    public bool ShowExplanation { get; set; } = true;

    [Column(TypeName = "jsonb")]
    public string? DifficultyDistribution { get; set; }

    [Column(TypeName = "jsonb")]
    public string? FilterTags { get; set; }

    public int TotalAttempts { get; set; } = 0;

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? AverageScore { get; set; }

    public int PassCount { get; set; } = 0;

    public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = [];
    public virtual ICollection<QuizAttempt> Attempts { get; set; } = [];
}
