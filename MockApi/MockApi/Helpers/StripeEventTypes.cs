namespace MockApi.Helpers
{
    public static class StripeEventTypes
    {
        public const string CheckoutSessionCompleted = "checkout.session.completed";
        public const string InvoicePaymentFailed = "invoice.payment_failed";
        public const string CustomerSubscriptionUpdated = "customer.subscription.updated";
        public const string CustomerSubscriptionDeleted = "customer.subscription.deleted";
    }
}
