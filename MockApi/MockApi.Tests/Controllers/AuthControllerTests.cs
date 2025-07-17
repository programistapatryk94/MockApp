using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockApi.Controllers;
using MockApi.Data;
using MockApi.Dtos.Auth;
using MockApi.Localization;
using MockApi.Models;
using MockApi.Runtime.Session;
using MockApi.Services;
using Moq;

namespace MockApi.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly AppDbContext _context;
        private readonly AuthController _controller;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<ITranslationService> _mockTranslationService;
        private readonly Mock<ILanguageManager> _mockLanguageManager;
        private readonly Mock<IAppSession> _mockAppSession;
        private readonly Mock<ILanguageService> _mockLanguageService;

        public AuthControllerTests()
        {
            // Mockowanie zależności
            _mockTokenService = new Mock<ITokenService>();
            _mockTranslationService = new Mock<ITranslationService>();
            _mockLanguageManager = new Mock<ILanguageManager>();
            _mockAppSession = new Mock<IAppSession>();
            _mockLanguageService = new Mock<ILanguageService>();

            // InMemory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "MockApiTestDb")
                .Options;

            _context = new AppDbContext(options, _mockAppSession.Object);

            _controller = new AuthController(
                _context,
                _mockTokenService.Object,
                _mockTranslationService.Object,
                _mockLanguageManager.Object,
                _mockAppSession.Object,
                _mockLanguageService.Object
            );
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenUserAlreadyExists()
        {
            // Arrange
            _context.Users.Add(new User
            {
                Email = "test@example.com",
                NormalizedEmailAddress = "TEST@EXAMPLE.COM"
            });
            await _context.SaveChangesAsync();

            var input = new CreateUserInput
            {
                Email = "test@example.com",
                Password = "Secret123!"
            };

            // Act
            var result = await _controller.Register(input);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequest.StatusCode);
        }

        [Fact]
        public async Task Register_ReturnsOk_WhenUserIsCreated()
        {
            // Arrange
            var input = new CreateUserInput
            {
                Email = "newuser@example.com",
                Password = "StrongPassword123!"
            };

            _mockTokenService.Setup(t => t.GenerateToken(It.IsAny<User>()))
                .Returns("mocked-token");

            // Act
            var result = await _controller.Register(input);

            // Assert
            var ok = Assert.IsType<ActionResult<RegisterResultDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(ok.Result);
            var tokenObj = Assert.IsType<RegisterResultDto>(okResult.Value);

            Assert.Equal("mocked-token", tokenObj.Token);

            var userInDb = await _context.Users.FirstOrDefaultAsync(u =>
                u.NormalizedEmailAddress == input.Email.ToUpperInvariant());

            Assert.NotNull(userInDb);
            Assert.Equal(input.Email.ToUpperInvariant(), userInDb.NormalizedEmailAddress);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenUserNotFound()
        {
            var input = new LoginUserInput
            {
                Email = "notfound@example.com",
                Password = "AnyPassword"
            };

            var result = await _controller.Login(input);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal(401, unauthorized.StatusCode);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenPasswordIsIncorrect()
        {
            _context.Users.Add(new User
            {
                Email = "user@example.com",
                NormalizedEmailAddress = "USER@EXAMPLE.COM",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword")
            });
            await _context.SaveChangesAsync();

            var input = new LoginUserInput
            {
                Email = "user@example.com",
                Password = "WrongPassword"
            };

            var result = await _controller.Login(input);
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);

            Assert.Equal(401, unauthorized.StatusCode);
        }

        [Fact]
        public async Task Login_ReturnsOk_WhenCredentialsAreCorrect()
        {
            var user = new User
            {
                Email = "user2@example.com",
                NormalizedEmailAddress = "USER2@EXAMPLE.COM",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Secret123")
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _mockTokenService.Setup(t => t.GenerateToken(It.IsAny<User>()))
                .Returns("mocked-token");

            var input = new LoginUserInput
            {
                Email = "user2@example.com",
                Password = "Secret123"
            };

            var result = await _controller.Login(input);

            var ok = Assert.IsType<ActionResult<LoginResultDto>>(result);
            var okResult = Assert.IsType<OkObjectResult>(ok.Result);
            var tokenObj = Assert.IsType<LoginResultDto>(okResult.Value);

            Assert.Equal("mocked-token", tokenObj.Token);
        }

        [Fact]
        public async Task ChangeLanguage_CallsService_AndReturnsOk()
        {
            var userId = Guid.NewGuid();

            _mockAppSession.SetupGet(x => x.UserId).Returns(userId); // zakładamy ID użytkownika

            _mockLanguageService.Setup(x => x.ChangeLanguageAsync("pl", userId)).Returns(Task.CompletedTask);

            var input = new ChangeLanguageInput { LanguageName = "pl" };

            var result = await _controller.ChangeLanguage(input);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Me_ReturnsCorrectLocalizationConfiguration()
        {
            // Arrange
            var currentCulture = new LanguageInfo("pl", "Polski", "flag-icon pl");

            var languages = new List<LanguageInfo>
    {
        new LanguageInfo("en", "English", "flag-icon us", isDefault: true),
        new LanguageInfo("pl", "Polski", "flag-icon pl")
    };

            _mockLanguageManager.SetupGet(x => x.CurrentLanguage).Returns(currentCulture);
            _mockLanguageManager.Setup(x => x.GetLanguages()).Returns(languages);

            // Act
            var result = await _controller.Me();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<UserInfoDto>(ok.Value);

            Assert.Equal("pl", dto.Localization.CurrentCulture.Name);
            Assert.Equal("Polski", dto.Localization.CurrentCulture.DisplayName);

            Assert.Equal(2, dto.Localization.Languages.Count);
            Assert.Contains(dto.Localization.Languages, l => l.DisplayName == "Polski");
            Assert.Contains(dto.Localization.Languages, l => l.DisplayName == "English");
        }
    }
}
