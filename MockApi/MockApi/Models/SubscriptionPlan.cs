namespace MockApi.Models
{
    public class SubscriptionPlan
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string StripePriceId { get; set; } = default!;
        public decimal Price { get; set; }
        public int MaxProjects { get; set; }
        public int MaxResources { get; set; }
        public bool HasCustomResponse { get; set; }
        public bool HasCollaboration { get; set; }
    }
}
