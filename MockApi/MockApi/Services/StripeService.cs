using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MockApi.Data;
using MockApi.Helpers;
using MockApi.Models;
using Stripe;
using Stripe.Checkout;

namespace MockApi.Services
{
    public class StripeService : IStripeService
    {
        private readonly StripeSettings _stripeSettings;
        private readonly string _clientRootAddress;
        private readonly AppDbContext _context;

        public StripeService(IOptions<StripeSettings> stripeOptions, IOptions<AppSettings> appSettings, AppDbContext context)
        {
            _stripeSettings = stripeOptions.Value;
            _clientRootAddress = appSettings.Value.ClientRootAddress;
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
            _context = context;
        }

        public Session CreateCheckoutSession(Guid userId)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "subscription",
                LineItems = new List<SessionLineItemOptions>
            {
                new() { Price = "price_1RjEJqBApjyVYR6tjpyKAKwR", Quantity = 1 }
            },
                SuccessUrl = $"{_clientRootAddress}/success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{_clientRootAddress}/cancel",
                Metadata = new Dictionary<string, string>
            {
                { "userId", userId.ToString() }
            }
            };

            var service = new SessionService();
            return service.Create(options);
        }

        public async Task HandleWebhookAsync(Event stripeEvent)
        {
            try
            {
                switch (stripeEvent.Type)
                {
                    // ✅ Gdy użytkownik zakończy Checkout (czyli płaci pierwszy raz)
                    case StripeEventTypes.CheckoutSessionCompleted:
                        {
                            var session = stripeEvent.Data.Object as Session;
                            var userIdStr = session.Metadata["userId"];

                            if (Guid.TryParse(userIdStr, out var userId))
                            {
                                var existing = await _context.Subscriptions
                                    .Include(s => s.History)
                                    .FirstOrDefaultAsync(s => s.UserId == userId);

                                if (existing != null)
                                {
                                    // Zamykamy ostatni status w historii
                                    var last = existing.History.OrderByDescending(h => h.CreatedAt).FirstOrDefault(h => h.EndedAt == null);
                                    if (last != null) last.EndedAt = DateTime.UtcNow;

                                    existing.StripeCustomerId = session.CustomerId;
                                    existing.StripeSubscriptionId = session.SubscriptionId;
                                    existing.Status = "active";

                                    _context.SubscriptionHistories.Add(new SubscriptionHistory
                                    {
                                        SubscriptionId = existing.Id,
                                        Status = "active",
                                        StripeSubscriptionId = session.SubscriptionId
                                    });
                                }
                                else
                                {
                                    var newSub = new Models.Subscription
                                    {
                                        UserId = userId,
                                        StripeCustomerId = session.CustomerId,
                                        StripeSubscriptionId = session.SubscriptionId,
                                        Status = "active"
                                    };

                                    _context.Subscriptions.Add(newSub);
                                    _context.SubscriptionHistories.Add(new SubscriptionHistory
                                    {
                                        SubscriptionId = newSub.Id,
                                        Status = "active",
                                        StripeSubscriptionId = session.SubscriptionId,
                                    });
                                }

                                await _context.SaveChangesAsync();
                                Console.WriteLine($"✅ Subskrypcja aktywowana dla userId: {userId}");
                            }
                            break;
                        }

                    // ✅ Gdy Stripe zmieni status subskrypcji (np. failed, unpaid, canceled, paused)
                    case StripeEventTypes.CustomerSubscriptionUpdated:
                        {
                            var stripeSub = stripeEvent.Data.Object as Stripe.Subscription;
                            var stripeSubId = stripeSub.Id;
                            var status = stripeSub.Status;

                            var sub = await _context.Subscriptions
                                .Include(s => s.History)
                                .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubId);

                            if (sub != null && sub.Status != status)
                            {
                                sub.Status = status;

                                var last = sub.History.OrderByDescending(h => h.CreatedAt).FirstOrDefault(h => h.EndedAt == null);
                                if (last != null) last.EndedAt = DateTime.UtcNow;

                                _context.SubscriptionHistories.Add(new SubscriptionHistory
                                {
                                    Id = Guid.NewGuid(),
                                    SubscriptionId = sub.Id,
                                    Status = status,
                                    StripeSubscriptionId = stripeSubId
                                });

                                await _context.SaveChangesAsync();
                                Console.WriteLine($"🔄 Status subskrypcji zmieniony na '{status}' dla userId: {sub.UserId}");
                            }

                            break;
                        }

                    // ✅ Gdy subskrypcja zostanie usunięta całkowicie
                    case StripeEventTypes.CustomerSubscriptionDeleted:
                        {
                            var stripeSub = stripeEvent.Data.Object as Stripe.Subscription;
                            var stripeSubId = stripeSub.Id;

                            var sub = await _context.Subscriptions
                                .Include(s => s.History)
                                .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubId);

                            if (sub != null)
                            {
                                sub.Status = "canceled";

                                var last = sub.History.OrderByDescending(h => h.CreatedAt).FirstOrDefault(h => h.EndedAt == null);
                                if (last != null) last.EndedAt = DateTime.UtcNow;

                                _context.SubscriptionHistories.Add(new SubscriptionHistory
                                {
                                    SubscriptionId = sub.Id,
                                    Status = "canceled",
                                    StripeSubscriptionId = stripeSubId,
                                });

                                await _context.SaveChangesAsync();
                                Console.WriteLine($"❌ Subskrypcja anulowana dla userId: {sub.UserId}");
                            }

                            break;
                        }

                    // (Opcjonalnie) Możesz też logować nieudane płatności, ale nie zmieniaj statusu tutaj
                    case StripeEventTypes.InvoicePaymentFailed:
                        {
                            var invoice = stripeEvent.Data.Object as Stripe.Invoice;
                            Console.WriteLine($"⚠️ Nieudana płatność dla sub: {invoice.Parent.SubscriptionDetails.SubscriptionId}");
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                var z = 5;
            }
        }
    }

}
