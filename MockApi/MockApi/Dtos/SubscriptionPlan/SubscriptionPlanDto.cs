namespace MockApi.Dtos.SubscriptionPlan
{
    public class SubscriptionPlanDto
    {
        public Guid Id { get; set; }
        public int MaxProjects { get; set; }
        public int MaxResources { get; set; }
        public bool HasCollaboration { get; set; }

        public List<SubscriptionPlanPriceDto> Prices { get; set; } = new();
    }
}
