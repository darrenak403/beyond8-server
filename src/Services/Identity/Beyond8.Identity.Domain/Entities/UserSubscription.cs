using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;
using Beyond8.Identity.Domain.Enums;

namespace Beyond8.Identity.Domain.Entities
{
    public class UserSubscription : BaseEntity
    {
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        public Guid PlanId { get; set; }

        [ForeignKey(nameof(PlanId))]
        public virtual SubscriptionPlan Plan { get; set; } = null!;

        public int TotalRemainingRequests { get; set; } = 35;
        public int RemainingRequestsPerWeek { get; set; } = 35;
        public DateTime? RequestLimitedEndsAt { get; set; } = null;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }

        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
        public Guid? OrderId { get; set; }
    }
}
