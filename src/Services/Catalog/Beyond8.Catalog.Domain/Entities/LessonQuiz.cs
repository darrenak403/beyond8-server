using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Catalog.Domain.Entities
{
    public class LessonQuiz : BaseEntity
    {
        public Guid LessonId { get; set; }
        [ForeignKey(nameof(LessonId))]
        public virtual Lesson Lesson { get; set; } = null!;

        public Guid? QuizId { get; set; }
    }
}