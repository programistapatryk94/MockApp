using MockApi.Runtime.DataModels;
using MockApi.Runtime.DataModels.Auditing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MockApi.Models
{
    public class SubscriptionHistory : AuditedEntity
    {
        [Required]
        public Guid SubscriptionId { get; set; }

        [ForeignKey("SubscriptionId")]
        public virtual Subscription Subscription { get; set; } = default!;

        [Required]
        public string Status { get; set; } = default!; // np. active, past_due, canceled

        [Required]
        public string StripeSubscriptionId { get; set; } = default!;

        public DateTime? EndedAt { get; set; }
    }
}
