using System.ComponentModel.DataAnnotations;

namespace MockApi.Models
{
    public class Subscription
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string StripeCustomerId { get; set; } = default!;

        [Required]
        public string StripeSubscriptionId { get; set; } = default!;

        [Required]
        public string Status { get; set; } = "active"; // np. active, past_due, canceled itd.

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual List<SubscriptionHistory> History { get; set; } = new();
    }
}
