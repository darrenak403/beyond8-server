using Beyond8.Common.Data.Base;
using Beyond8.Identity.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Beyond8.Identity.Domain.Entities
{
    public class InstructorProfile : BaseEntity
    {
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [MaxLength(300)]
        public string? Bio { get; set; }

        [MaxLength(200)]
        public string? Headline { get; set; }
        [Column(TypeName = "jsonb")]
        public string? ExpertiseAreas { get; set; }

        [Column(TypeName = "jsonb")]
        public string? Education { get; set; }

        [Column(TypeName = "jsonb")]
        public string? WorkExperience { get; set; }

        [Column(TypeName = "jsonb")]
        public string? SocialLinks { get; set; }
        public string? BankInfo { get; set; }
        public string? TaxId { get; set; }

        [Column(TypeName = "jsonb")]
        public string? IdentityDocuments { get; set; }

        [Column(TypeName = "jsonb")]
        public string? Certificates { get; set; }
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
        public string? VerificationNotes { get; set; }
        public Guid? VerifiedBy { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public int TotalStudents { get; set; } = 0;
        public int TotalCourses { get; set; } = 0;

        [Column(TypeName = "decimal(3, 2)")]
        public decimal? AvgRating { get; set; }
    }
}