using MockApi.Runtime.DataModels;

namespace MockApi.Models
{
    public class SubscriptionPlanPrice : Entity
    {
        public string Name { get; set; } = default!;
        public string Currency { get; set; } = default!; // np. "PLN", "USD"
        public decimal Amount { get; set; }

        public string StripePriceId { get; set; } = default!; // przypisany do danej waluty

        public Guid SubscriptionPlanId { get; set; }
        public virtual SubscriptionPlan SubscriptionPlan { get; set; } = default!;

        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
