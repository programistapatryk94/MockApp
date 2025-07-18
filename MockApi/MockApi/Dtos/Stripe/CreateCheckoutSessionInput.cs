namespace MockApi.Dtos.Stripe
{
    public class CreateCheckoutSessionInput
    {
        public Guid SubscriptionPlanPriceId { get; set; }
    }
}
