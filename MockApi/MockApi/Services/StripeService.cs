using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MockApi.Data;
using MockApi.Helpers;
using MockApi.Localization;
using MockApi.Models;
using MockApi.Runtime.Exceptions;
using Stripe;
using Stripe.Checkout;

namespace MockApi.Services
{
    public class StripeService : IStripeService
    {
        private readonly StripeSettings _stripeSettings;
        private readonly string _clientRootAddress;
        private readonly AppDbContext _context;
        private readonly ILogger<StripeService> _logger;
        private readonly ITranslationService _translationService;

        public StripeService(IOptions<StripeSettings> stripeOptions, IOptions<AppSettings> appSettings, AppDbContext context, ILogger<StripeService> logger, ITranslationService translationService)
        {
            _stripeSettings = stripeOptions.Value;
            _clientRootAddress = appSettings.Value.ClientRootAddress;
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
            _context = context;
            _logger = logger;
            _translationService = translationService;
        }

        public async Task<Session> CreateCheckoutSessionAsync(Guid userId, Guid subsriptionPlanPriceId)
        {
            var planPrice = await _context.SubscriptionPlanPrices.FirstOrDefaultAsync(p => p.Id == subsriptionPlanPriceId);
            if(null == planPrice)
            {
                throw new UserFriendlyException(_translationService.Translate("PlanPriceNotFound"));
            }

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "subscription",
                LineItems = new List<SessionLineItemOptions>
            {
                new() { Price = planPrice.StripePriceId, Quantity = 1 }
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
                            await HandleCheckoutSessionCompleted(stripeEvent);
                            break;
                        }

                    // ✅ Gdy Stripe zmieni status subskrypcji (np. failed, unpaid, canceled, paused)
                    case StripeEventTypes.CustomerSubscriptionUpdated:
                        {
                            await HandleSubscriptionUpdated(stripeEvent);
                            break;
                        }

                    // ✅ Gdy subskrypcja zostanie usunięta całkowicie
                    case StripeEventTypes.CustomerSubscriptionDeleted:
                        {
                            await HandleSubscriptionDeleted(stripeEvent);
                            break;
                        }

                    // (Opcjonalnie) Możesz też logować nieudane płatności, ale nie zmieniaj statusu tutaj
                    case StripeEventTypes.InvoicePaymentFailed:
                        {
                            var invoice = stripeEvent.Data.Object as Stripe.Invoice;
                            var subscriptionId = invoice.Parent?.SubscriptionDetails?.SubscriptionId;
                            _logger.LogWarning("⚠️ Nieudana płatność. SubscriptionId: {SubscriptionId}", subscriptionId ?? "Brak");
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Błąd podczas obsługi webhooka Stripe. EventType: {EventType}, EventId: {EventId}",
                    stripeEvent?.Type,
                    stripeEvent?.Id);
            }
        }

        private async Task HandleSubscriptionDeleted(Event stripeEvent)
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
                _logger.LogInformation("Subskrypcja anulowana dla userId: {UserId}", sub.UserId);
            }
        }

        private async Task HandleSubscriptionUpdated(Event stripeEvent)
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
                _logger.LogInformation("Status subskrypcji zmieniony na '{Status}' dla userId: {UserId}", status, sub.UserId);
            }
        }

        private async Task HandleCheckoutSessionCompleted(Event stripeEvent)
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
                _logger.LogInformation("Subskrypcja aktywowana dla userId: {UserId}", userId);
            }
        }
    }

}
