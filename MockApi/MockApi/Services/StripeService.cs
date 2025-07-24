using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MockApi.Data;
using MockApi.Helpers;
using MockApi.Localization;
using MockApi.Models;
using MockApi.Runtime.Exceptions;
using MockApi.Services.Models;
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
        private readonly IUserManager _userManager;

        public StripeService(IOptions<StripeSettings> stripeOptions, IOptions<AppSettings> appSettings, AppDbContext context, ILogger<StripeService> logger, ITranslationService translationService, IUserManager userManager)
        {
            _stripeSettings = stripeOptions.Value;
            _clientRootAddress = appSettings.Value.ClientRootAddress;
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
            _context = context;
            _logger = logger;
            _translationService = translationService;
            _userManager = userManager;
        }

        public async Task<Session> CreateCheckoutSessionAsync(Guid userId, Guid subscriptionPlanPriceId)
        {
            var planPrice = await _context.SubscriptionPlanPrices.FirstOrDefaultAsync(p => p.Id == subscriptionPlanPriceId);
            if (planPrice == null)
            {
                throw new UserFriendlyException(_translationService.Translate("PlanPriceNotFound"));
            }

            if (await _context.Subscriptions.AnyAsync(p => p.UserId == userId && p.Status != "canceled"))
            {
                throw new UserFriendlyException("Subskrypcja istnieje");
            }

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "subscription",
                LineItems = new List<SessionLineItemOptions>
                {
                    new() { Price = planPrice.StripePriceId, Quantity = 1 }
                },
                SuccessUrl = $"{_clientRootAddress}/payments/success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{_clientRootAddress}/payments/cancel",
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId.ToString() },
                    { "subscriptionPlanPriceId", subscriptionPlanPriceId.ToString() }
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
                            _logger.LogWarning("Nieudana płatność. SubscriptionId: {SubscriptionId}", subscriptionId ?? "Brak");
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

                await RevokePremiumAsync(sub.UserId, save: false);

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
            var subscriptionPlanPriceIdStr = session.Metadata["subscriptionPlanPriceId"];

            if (Guid.TryParse(userIdStr, out var userId) && Guid.TryParse(subscriptionPlanPriceIdStr, out var subscriptionPlanPriceId))
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
                    var newSub = new MockApi.Models.Subscription
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

                await GivePremiumAsync(userId, subscriptionPlanPriceId, session.SubscriptionId, save: false);

                await _context.SaveChangesAsync();
                _logger.LogInformation("Subskrypcja aktywowana dla userId: {UserId}", userId);
            }
        }

        private async Task GivePremiumAsync(Guid userId, Guid subscriptionPlanPriceId, string stripeSubId, bool save = true)
        {
            var user = await _context.Users.FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null)
            {
                _logger.LogInformation("[GivePremiumAsync] Użytkownik nie istnieje dla userId: {UserId}", userId);
                return;
            }

            var subscriptionPlanPrice = await _context.SubscriptionPlanPrices
                .Where(p => p.Id == subscriptionPlanPriceId)
                .Select(p => new
                {
                    p.Id,
                    p.SubscriptionPlan,
                }).FirstOrDefaultAsync();
            if (subscriptionPlanPrice == null)
            {
                _logger.LogInformation("[GivePremiumAsync] Plan subskrypcji nie istnieje dla subscriptionPlanPriceId: {SubscriptionPlanPriceId}", subscriptionPlanPriceId);
                return;
            }

            var subscriptionPlan = subscriptionPlanPrice.SubscriptionPlan;

            if (user.SubscriptionPlanPriceId.HasValue && user.SubscriptionPlanPriceId == subscriptionPlanPriceId)
            {
                _logger.LogInformation("[GivePremiumAsync] użytkownik ma przypisane konto premium");
                return;
            }

            await _userManager.SetFeatureValuesAsync(userId, new Dictionary<string, string>
            {
                { AppFeatures.CollaborationEnabled, subscriptionPlan.HasCollaboration.ToString().ToLower() },
                { AppFeatures.MaxMockCreationLimit, subscriptionPlan.MaxResources.ToString() },
                { AppFeatures.MaxProjectCreationLimit, subscriptionPlan.MaxProjects.ToString() }
            });

            user.SubscriptionPlanPriceId = subscriptionPlanPriceId;
            user.IsCollaborationEnabled = subscriptionPlan.HasCollaboration;

            var stripeSubService = new Stripe.SubscriptionService();
            var stripeSub = await stripeSubService.GetAsync(stripeSubId);

            var subscriptionItem = stripeSub.Items.Data.FirstOrDefault();

            DateTime endDate = subscriptionItem?.CurrentPeriodEnd ?? stripeSub.BillingCycleAnchor;

            var existing = await _context.CurrentSubscriptions.FirstOrDefaultAsync(x => x.UserId == userId);
            if (existing == null)
            {
                _context.CurrentSubscriptions.Add(new CurrentSubscription
                {
                    UserId = userId,
                    SubscriptionPlanPriceId = subscriptionPlanPriceId,
                    IsCanceling = false,
                    ValidUntil = null,
                    BillingCycleAnchor = endDate
                });
            }
            else
            {
                // Aktualizacja istniejącej subskrypcji (np. przedłużenie lub nowa po anulowaniu)
                existing.SubscriptionPlanPriceId = subscriptionPlanPriceId;
                existing.IsCanceling = false;
                existing.ValidUntil = null;
                existing.BillingCycleAnchor = endDate;
            }

            if (save)
            {
                await _context.SaveChangesAsync();
            }
        }

        private async Task RevokePremiumAsync(Guid userId, bool save = true)
        {
            var user = await _context.Users
                .Include(p => p.CurrentSubscription)
                .FirstOrDefaultAsync(p => p.Id == userId);
            if (user == null)
            {
                _logger.LogInformation("[RevokePremiumAsync] Użytkownik nie istnieje dla userId: {UserId}", userId);
                return;
            }

            if (user.SubscriptionPlanPriceId == null || user.CurrentSubscription == null)
            {
                _logger.LogInformation("[RevokePremiumAsync] Użytkownik miał odebrany plan subskrypcji wcześniej: {UserId}", userId);
                return;
            }


            await _userManager.RemoveFeaturesAsync(userId, new List<string>
            {
                AppFeatures.CollaborationEnabled,
                AppFeatures.MaxMockCreationLimit,
                AppFeatures.MaxProjectCreationLimit
            });

            user.SubscriptionPlanPriceId = null;
            user.IsCollaborationEnabled = false;
            _context.CurrentSubscriptions.Remove(user.CurrentSubscription);

            if (save)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task CancelSubscriptionAtPeriodEndAsync(Guid userId)
        {
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Status == "active");

            var currentSubscription = await _context.CurrentSubscriptions
               .Include(cs => cs.SubscriptionPlanPrice)
               .FirstOrDefaultAsync(cs => cs.UserId == userId);

            if (subscription == null || currentSubscription == null || currentSubscription.IsCanceling)
            {
                throw new UserFriendlyException(_translationService.Translate("ActiveSubscriptionToCancelNotFound"));
            }

            var service = new Stripe.SubscriptionService();
            var options = new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = true,
            };

            var updatedSubscription = await service.UpdateAsync(subscription.StripeSubscriptionId, options);

            currentSubscription.IsCanceling = true;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Ustawiono anulowanie subskrypcji na koniec okresu dla userId: {UserId}", userId);
        }

        public async Task<PaymentInfo> GetPaymentInfo(Guid userId, string sessionId)
        {
            var sessionService = new SessionService();
            var session = await sessionService.GetAsync(sessionId);

            if (session == null)
            {
                throw new UserFriendlyException(_translationService.Translate("StripeSessionNotFound"));
            }

            var sessionUserId = Guid.Parse(session.Metadata["userId"]);
            var subscriptionPlanPriceId = Guid.Parse(session.Metadata["subscriptionPlanPriceId"]);

            if(sessionUserId != userId)
            {
                throw new UserFriendlyException(_translationService.Translate("StripeSessionNotFound"));
            }

            return new PaymentInfo
            {
                PaymentStatus = session.PaymentStatus
            };
        }
    }

}
