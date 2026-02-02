using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Catalog.Domain.Enums;
using Beyond8.Common.Data.Base;

namespace Beyond8.Catalog.Domain.Entities
{
    public class Lesson : BaseEntity
    {
        public Guid SectionId { get; set; }
        [ForeignKey(nameof(SectionId))]
        public virtual Section Section { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public LessonType Type { get; set; } = LessonType.Video;

        public int OrderIndex { get; set; }

        public bool IsPreview { get; set; } = false;
        public bool IsPublished { get; set; } = true;

        // Statistics
        public int TotalViews { get; set; } = 0;
        public int TotalCompletions { get; set; } = 0;

        // Relationships
        public virtual ICollection<LessonDocument> Documents { get; set; } = [];

        // Navigation to specific lesson types
        public virtual LessonVideo? Video { get; set; }
        public virtual LessonText? Text { get; set; }
        public virtual LessonQuiz? Quiz { get; set; }
    }
}