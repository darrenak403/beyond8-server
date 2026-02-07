using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Analytic.Domain.Entities
{
    public class AggCourseStats : BaseEntity
    {
        public Guid CourseId { get; set; }

        [Required, MaxLength(200)]
        public string CourseTitle { get; set; } = string.Empty;

        public Guid InstructorId { get; set; }

        [MaxLength(200)]
        public string InstructorName { get; set; } = string.Empty;

        public int TotalStudents { get; set; } = 0;
        public int TotalCompletedStudents { get; set; } = 0;

        [Column(TypeName = "decimal(5, 2)")]
        public decimal CompletionRate { get; set; } = 0;

        [Column(TypeName = "decimal(3, 2)")]
        public decimal? AvgRating { get; set; }

        public int TotalReviews { get; set; } = 0;
        public int TotalRatings { get; set; } = 0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalRevenue { get; set; } = 0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalRefundAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal NetRevenue { get; set; } = 0;

        public int TotalViews { get; set; } = 0;

        [Column(TypeName = "decimal(5, 2)")]
        public decimal AvgWatchTime { get; set; } = 0;
    }
}