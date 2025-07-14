using MockApi.Runtime.DataModels.Auditing;
using System.ComponentModel.DataAnnotations;

namespace MockApi.Models
{
    public class Subscription : AuditedEntity
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string StripeCustomerId { get; set; } = default!;

        [Required]
        public string StripeSubscriptionId { get; set; } = default!;

        [Required]
        public string Status { get; set; } = "active"; // np. active, past_due, canceled itd.

        public virtual List<SubscriptionHistory> History { get; set; } = new();
    }
}
