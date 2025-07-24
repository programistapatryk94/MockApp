using MockApi.Services.Models;
using Stripe;
using Stripe.Checkout;

namespace MockApi.Services
{
    public interface IStripeService
    {
        Task<Session> CreateCheckoutSessionAsync(Guid userId, Guid subsriptionPlanPriceId);
        Task HandleWebhookAsync(Event stripeEvent);
        Task CancelSubscriptionAtPeriodEndAsync(Guid userId);
        Task<PaymentInfo> GetPaymentInfo(Guid userId, string sessionId);
    }
}
