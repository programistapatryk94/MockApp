namespace MockApi.Dtos.SubscriptionPlan
{
    public class SubscriptionPlanPriceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Currency { get; set; } = default!; // np. "PLN", "USD"
        public decimal Amount { get; set; }
    }
}
