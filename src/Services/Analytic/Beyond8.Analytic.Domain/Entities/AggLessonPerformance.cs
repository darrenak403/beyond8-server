using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Analytic.Domain.Entities
{
    public class AggLessonPerformance : BaseEntity
    {
        public Guid LessonId { get; set; }

        [Required, MaxLength(200)]
        public string LessonTitle { get; set; } = string.Empty;

        public Guid CourseId { get; set; }
        public Guid InstructorId { get; set; }

        public int TotalViews { get; set; } = 0;
        public int UniqueViewers { get; set; } = 0;

        public int TotalCompletions { get; set; } = 0;

        [Column(TypeName = "decimal(5, 2)")]
        public decimal CompletionRate { get; set; } = 0;

        [Column(TypeName = "decimal(5, 2)")]
        public decimal AvgWatchPercent { get; set; } = 0;

        public int AvgWatchTimeSeconds { get; set; } = 0;

        [Column(TypeName = "jsonb")]
        public string? DropOffPoints { get; set; }
    }
}