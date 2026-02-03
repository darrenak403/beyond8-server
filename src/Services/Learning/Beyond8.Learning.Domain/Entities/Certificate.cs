using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Learning.Domain.Entities;

public class Certificate : BaseEntity
{
    public Guid EnrollmentId { get; set; }
    [ForeignKey(nameof(EnrollmentId))]
    public virtual Enrollment Enrollment { get; set; } = null!;

    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }

    [Required, MaxLength(100)]
    public string CertificateNumber { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string StudentName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string CourseTitle { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string InstructorName { get; set; } = string.Empty;

    public DateTime CompletionDate { get; set; }
    public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

    public string? CertificatePdfUrl { get; set; }
    public string? CertificateImageUrl { get; set; }

    [Required, MaxLength(64)]
    public string VerificationHash { get; set; } = string.Empty;

    public bool IsValid { get; set; } = true;
    public DateTime? RevokedAt { get; set; }

    [MaxLength(500)]
    public string? RevocationReason { get; set; }
}