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

        public StripeController(IOptions<StripeSettings> stripeSettings, IAppSession appSession, IStripeService stripeService)
        {
            _stripeSettings = stripeSettings.Value;
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
            _appSession = appSession;
            _stripeService = stripeService;
        }

        [Authorize]
        [HttpPost("create-checkout-session")]
        public ActionResult<CheckoutSessionDto> CreateCheckoutSession()
        {
            var session = _stripeService.CreateCheckoutSession(_appSession.UserId!.Value);

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
                //Console.WriteLine($"❌ Stripe error: {ex.Message}");
                return BadRequest();
            }

            await _stripeService.HandleWebhookAsync(stripeEvent);

            return Ok();
        }
    }
}
