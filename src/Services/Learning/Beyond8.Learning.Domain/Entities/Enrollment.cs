using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;
using Beyond8.Learning.Domain.Enums;

namespace Beyond8.Learning.Domain.Entities
{
    public class Enrollment : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid CourseId { get; set; }

        [Required, MaxLength(200)]
        public string CourseTitle { get; set; } = string.Empty;

        public string? CourseThumbnailUrl { get; set; }

        public Guid InstructorId { get; set; }

        [MaxLength(200)]
        public string InstructorName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PricePaid { get; set; } = 0;

        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

        [Column(TypeName = "decimal(5, 2)")]
        public decimal ProgressPercent { get; set; } = 0;

        public int CompletedLessons { get; set; } = 0;
        public int TotalLessons { get; set; } = 0;

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public DateTime? CertificateIssuedAt { get; set; }

        public Guid? CertificateId { get; set; }

        public DateTime? LastAccessedAt { get; set; }
        public Guid? LastAccessedLessonId { get; set; }

        public virtual ICollection<LessonProgress> LessonProgresses { get; set; } = [];
        public virtual ICollection<SectionProgress> SectionProgresses { get; set; } = [];
        public virtual ICollection<LessonNote> Notes { get; set; } = [];
        public virtual CourseReview? Review { get; set; }
        public virtual Certificate? Certificate { get; set; }
    }
}