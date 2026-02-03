using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Learning.Domain.Entities;

public class CourseReview : BaseEntity
{
    public Guid CourseId { get; set; }
    public Guid UserId { get; set; }
    public Guid EnrollmentId { get; set; }

    [ForeignKey(nameof(EnrollmentId))]
    public virtual Enrollment Enrollment { get; set; } = null!;

    public int Rating { get; set; }

    [MaxLength(2000)]
    public string? Review { get; set; }

    public int? ContentQuality { get; set; }
    public int? InstructorQuality { get; set; }
    public int? ValueForMoney { get; set; }

    public bool IsVerifiedPurchase { get; set; } = true;
    public bool IsPublished { get; set; } = true;

    public int HelpfulCount { get; set; } = 0;
    public int NotHelpfulCount { get; set; } = 0;

    public bool IsFlagged { get; set; } = false;

    [MaxLength(500)]
    public string? FlagReason { get; set; }

    public Guid? ModeratedBy { get; set; }
    public DateTime? ModeratedAt { get; set; }
}
