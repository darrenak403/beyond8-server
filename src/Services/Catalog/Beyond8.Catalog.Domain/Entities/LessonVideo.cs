using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Catalog.Domain.Entities
{
    public class LessonVideo : BaseEntity
    {
        public Guid LessonId { get; set; }
        [ForeignKey(nameof(LessonId))]
        public virtual Lesson Lesson { get; set; } = null!;

        [Column(TypeName = "jsonb")]
        public string? HlsVariants { get; set; }

        public string? VideoOriginalUrl { get; set; }

        public string? VideoThumbnailUrl { get; set; }

        public int? DurationSeconds { get; set; }

        [Column(TypeName = "jsonb")]
        public string? VideoQualities { get; set; }

        public bool IsDownloadable { get; set; } = false;
    }
}