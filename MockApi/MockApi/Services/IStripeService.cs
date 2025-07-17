using Stripe;
using Stripe.Checkout;

namespace MockApi.Services
{
    public interface IStripeService
    {
        Session CreateCheckoutSession(Guid userId);
        Task HandleWebhookAsync(Event stripeEvent);
    }
}
