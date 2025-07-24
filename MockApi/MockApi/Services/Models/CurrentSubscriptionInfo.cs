namespace MockApi.Services.Models
{
    public class CurrentSubscriptionInfo
    {
        public Guid Id { get; set; }
        public int MaxProjects { get; set; }
        public int MaxResources { get; set; }
        public bool HasCollaboration { get; set; }
        public bool IsCanceling { get; set; }
        public DateTime CurrentPeriodEnd { get; set; }

        public List<CurrentSubscriptionPlanPriceInfo> Prices { get; set; } = new();
    }

    public class CurrentSubscriptionPlanPriceInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Currency { get; set; } = default!;
        public decimal Amount { get; set; }
    }
}
