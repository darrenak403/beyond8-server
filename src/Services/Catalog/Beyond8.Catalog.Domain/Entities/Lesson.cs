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

        [Column(TypeName = "jsonb")]
        public string? HlsVariants { get; set; }
        public string? VideoOriginalUrl { get; set; }
        public string? VideoThumbnailUrl { get; set; }

        public int? DurationSeconds { get; set; }

        [Column(TypeName = "jsonb")]
        public string? VideoQualities { get; set; }
        public bool IsDownloadable { get; set; } = false;

        // Lesson Type Text
        public string? TextContent { get; set; }

        // Lesson Type Quiz
        public Guid? QuizId { get; set; }
        public int MinCompletionSeconds { get; set; } = 0;
        public int RequiredScore { get; set; } = 0;

        // Statistics
        public int TotalViews { get; set; } = 0;
        public int TotalCompletions { get; set; } = 0;

        // Relationships
        public virtual ICollection<LessonDocument> Documents { get; set; } = [];
    }
}