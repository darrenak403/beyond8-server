using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Identity.Domain.Entities
{
    public class SubscriptionPlan : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = null!; // "FREE", "PLUS", "PRO"

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; } = 0; // 0 = Free

        [MaxLength(10)]
        public string Currency { get; set; } = "VND";

        /// <summary>0 = Free (7 days from signup), 30 = monthly.</summary>
        public int DurationDays { get; set; } = 0;

        /// <summary>Total requests in period: Free=35, Plus=200, Pro=400.</summary>
        public int TotalRequestsInPeriod { get; set; } = 35;

        /// <summary>Max requests per week: Free=35, Plus=50, Pro=100.</summary>
        public int MaxRequestsPerWeek { get; set; } = 35;

        /// <summary>Danh sách quyền lợi hiển thị cho frontend (ví dụ: "35 request AI/7 ngày", "Hỗ trợ email").</summary>
        public List<string> Includes { get; set; } = [];

        public bool IsActive { get; set; } = true;

        public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = [];
    }
}
