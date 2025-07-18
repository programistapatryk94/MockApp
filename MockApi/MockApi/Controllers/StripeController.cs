using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MockApi.Dtos.Stripe;
using MockApi.Helpers;
using MockApi.Runtime.Session;
using MockApi.Services;
using Stripe;

namespace MockApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StripeController : ControllerBase
    {
        private readonly StripeSettings _stripeSettings;
        private readonly IAppSession _appSession;
        private readonly IStripeService _stripeService;
        private readonly ILogger<StripeController> _logger;

        public StripeController(IOptions<StripeSettings> stripeSettings, IAppSession appSession, IStripeService stripeService, ILogger<StripeController> logger)
        {
            _stripeSettings = stripeSettings.Value;
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
            _appSession = appSession;
            _stripeService = stripeService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("create-checkout-session")]
        public async Task<ActionResult<CheckoutSessionDto>> CreateCheckoutSession([FromBody] CreateCheckoutSessionInput input)
        {
            var session = await _stripeService.CreateCheckoutSessionAsync(_appSession.UserId!.Value, input.SubscriptionPlanPriceId);

            return Ok(new CheckoutSessionDto { Url = session.Url });
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _stripeSettings.WebhookSecret
                );
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Błąd Stripe podczas weryfikacji webhooka: {Message}", ex.Message);
                return BadRequest();
            }

            await _stripeService.HandleWebhookAsync(stripeEvent);

            return Ok();
        }
    }
}
