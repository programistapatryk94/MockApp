using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MockApi.Controllers;
using MockApi.Dtos.Stripe;
using MockApi.Helpers;
using MockApi.Runtime.Session;
using MockApi.Services;
using Moq;
using Stripe.Checkout;
using System.Text;

namespace MockApi.Tests.Controllers
{
    public class StripeControllerTests
    {
        private readonly StripeController _controller;
        private readonly Mock<IStripeService> _mockStripeService = new();
        private readonly Mock<IAppSession> _mockAppSession = new();
        private readonly StripeSettings _stripeSettings = new()
        {
            SecretKey = "sk_test_dummy",
            WebhookSecret = "whsec_dummy"
        };

        public StripeControllerTests()
        {
            var optionsStripe = Options.Create(_stripeSettings);

            _controller = new StripeController(
                optionsStripe,
                _mockAppSession.Object,
                _mockStripeService.Object
            );
        }

        [Fact]
        public void CreateCheckoutSession_ReturnsOkWithUrl()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockAppSession.Setup(s => s.UserId).Returns(userId);

            var fakeSession = new Session { Url = "https://stripe.com/checkout" };
            _mockStripeService.Setup(s => s.CreateCheckoutSession(userId))
                .Returns(fakeSession);

            // Act
            var result = _controller.CreateCheckoutSession();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var payload = Assert.IsType<CheckoutSessionDto>(ok.Value);
            Assert.Equal("https://stripe.com/checkout", payload.Url);
        }

        [Fact]
        public async Task Webhook_InvalidSignature_ReturnsBadRequest()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var json = "{}";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
            context.Request.Headers["Stripe-Signature"] = "invalid";
            _controller.ControllerContext = new ControllerContext { HttpContext = context };

            // Act
            var result = await _controller.Webhook();

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }
    }
}
