using MockApi.Runtime.DataModels.Auditing;

namespace MockApi.Models
{
    public class CurrentSubscription : AuditedEntity<long>
    {
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = default!;

        public Guid? SubscriptionPlanPriceId { get; set; }
        public virtual SubscriptionPlanPrice? SubscriptionPlanPrice { get; set; }

        public bool IsCanceling { get; set; } = false;

        public DateTime? ValidUntil { get; set; }

        public DateTime BillingCycleAnchor { get; set; }
    }
}
