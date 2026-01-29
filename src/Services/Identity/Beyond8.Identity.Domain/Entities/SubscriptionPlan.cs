using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Identity.Domain.Entities
{
    public class SubscriptionPlan : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = null!; // "FREE", "PLUS", "PRO", "ULTRA"

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; } = 0; // 0 = Free

        [MaxLength(10)]
        public string Currency { get; set; } = "VND";

        public int DurationDays { get; set; } = 0;

        public int TotalRequestsInPeriod { get; set; } = 35;

        public int MaxRequestsPerWeek { get; set; } = 35;

        public List<string> Includes { get; set; } = [];

        public bool IsActive { get; set; } = true;

        public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = [];
    }
}
