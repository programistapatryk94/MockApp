using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MockApi.Helpers;
using MockApi.Localization;
using MockApi.Localization.RequestCulture;
using MockApi.Runtime.Features;
using MockApi.Runtime.Session;
using Moq;

namespace MockApi.Tests.Localization.RequestCulture
{
    public class UserRequestCultureProviderTests
    {
        private readonly Mock<ILogger<UserRequestCultureProvider>> _mockLogger = new();
        private readonly Mock<IAppSession> _mockSession = new();
        private readonly Mock<IFeatureChecker> _mockFeatureChecker = new();
        private readonly Mock<ILanguageManager> _mockLanguageManager = new();
        private readonly Mock<ILanguageService> _mockLanguageService = new();

        private HttpContext CreateHttpContext()
        {
            var context = new DefaultHttpContext();

            var services = new ServiceCollection();
            services.AddSingleton(_mockLanguageManager.Object);
            services.AddSingleton(_mockLanguageService.Object);
            services.AddSingleton(_mockSession.Object);
            services.AddSingleton(_mockFeatureChecker.Object);
            context.RequestServices = services.BuildServiceProvider();

            return context;
        }

        [Fact]
        public async Task DetermineProviderCultureResult_ReturnsCultureFromFeatureChecker()
        {
            // Arrange
            var provider = new UserRequestCultureProvider(_mockLogger.Object);
            var httpContext = CreateHttpContext();

            _mockSession.Setup(s => s.UserId).Returns(Guid.NewGuid());
            _mockFeatureChecker
                .Setup(f => f.GetValueAsync(AppFeatures.DefaultLanguage, false))
                .ReturnsAsync("pl");

            // Act
            var result = await provider.DetermineProviderCultureResult(httpContext);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("pl", result!.Cultures[0].Value);
            Assert.Equal("pl", result.UICultures[0].Value);
        }

        [Fact]
        public async Task DetermineProviderCultureResult_ReturnsNull_WhenUserIsNotLoggedIn()
        {
            // Arrange
            var provider = new UserRequestCultureProvider(_mockLogger.Object);
            var httpContext = CreateHttpContext();

            _mockSession.SetupGet(s => s.UserId).Returns((Guid?)null);

            // Act
            var result = await provider.DetermineProviderCultureResult(httpContext);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DetermineProviderCultureResult_ReturnsNull_WhenNoCultureFound()
        {
            // Arrange
            var provider = new UserRequestCultureProvider(_mockLogger.Object);
            var httpContext = CreateHttpContext();

            _mockSession.Setup(s => s.UserId).Returns(Guid.NewGuid());
            _mockFeatureChecker.Setup(f => f.GetValueAsync(It.IsAny<string>(), false))
                .ReturnsAsync((string?)null);

            // Act
            var result = await provider.DetermineProviderCultureResult(httpContext);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DetermineProviderCultureResult_ReturnsCultureFromCookie_WhenNoUserCulture()
        {
            // Arrange
            _mockLanguageManager.Setup(m => m.GetLanguages()).Returns(new List<LanguageInfo>());
            var provider = new UserRequestCultureProvider(_mockLogger.Object)
            {
                CookieProvider = new CookieRequestCultureProvider()
            };

            var context = CreateHttpContext();

            // Wstawiamy nagłówek Cookie, który ASP.NET Core sparsuje jako Cookies
            var cultureCookie = CookieRequestCultureProvider.MakeCookieValue(new Microsoft.AspNetCore.Localization.RequestCulture("fr"));
            context.Request.Headers.Add("Cookie", $"{CookieRequestCultureProvider.DefaultCookieName}={cultureCookie}");

            _mockSession.Setup(s => s.UserId).Returns(Guid.NewGuid());
            _mockFeatureChecker.Setup(f => f.GetValueAsync(It.IsAny<string>(), false))
                .ReturnsAsync((string?)null);

            // Act
            var result = await provider.DetermineProviderCultureResult(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("fr", result!.Cultures[0].Value);
            Assert.Equal("fr", result.UICultures[0].Value);
        }

        [Fact]
        public async Task DetermineProviderCultureResult_ReturnsCultureFromHeader_WhenNoUserCultureAndNoCookie()
        {
            // Arrange
            _mockLanguageManager.Setup(m => m.GetLanguages()).Returns(new List<LanguageInfo>());
            Mock<ILogger<LocalizationHeaderRequestCultureProvider>> mockLogger = new();
            var provider = new UserRequestCultureProvider(_mockLogger.Object)
            {
                HeaderProvider = new LocalizationHeaderRequestCultureProvider(mockLogger.Object)
            };

            var context = CreateHttpContext();
            context.Request.Headers[".AspNetCore.Culture"] = "c=de-DE|uic=de-DE";

            _mockSession.Setup(s => s.UserId).Returns(Guid.NewGuid());
            _mockFeatureChecker.Setup(f => f.GetValueAsync(It.IsAny<string>(), false))
                .ReturnsAsync((string?)null);

            // Act
            var result = await provider.DetermineProviderCultureResult(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("de-DE", result!.Cultures[0].Value);
            Assert.Equal("de-DE", result.UICultures[0].Value);
        }

    }
}
