using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Assessment.Domain.Enums;
using Beyond8.Common.Data.Base;

namespace Beyond8.Assessment.Domain.Entities;

public class Question : BaseEntity
{
    public Guid InstructorId { get; set; }

    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public QuestionType Type { get; set; } = QuestionType.MultipleChoice;

    [Column(TypeName = "jsonb")]
    public string Options { get; set; } = "[]"; // [{"id":"a","text":"Option 1","isCorrect":true}]

    [MaxLength(2000)]
    public string? Explanation { get; set; }

    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;

    public int Points { get; set; } = 1;

    [Column(TypeName = "jsonb")]
    public string Tags { get; set; } = "[]";

    public bool IsActive { get; set; } = true;

    public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = [];
}
