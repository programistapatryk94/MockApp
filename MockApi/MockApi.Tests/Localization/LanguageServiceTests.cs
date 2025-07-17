using Microsoft.EntityFrameworkCore;
using MockApi.Data;
using MockApi.Helpers;
using MockApi.Localization;
using MockApi.Runtime.Features;
using MockApi.Runtime.Session;
using Moq;

namespace MockApi.Tests.Localization
{
    public class LanguageServiceTests
    {
        private readonly AppDbContext _context;
        private readonly Mock<IFeatureManager> _mockFeatureManager = new();
        private readonly LanguageService _service;
        private readonly Guid _userId = Guid.NewGuid();

        public LanguageServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options, Mock.Of<IAppSession>());
            _mockFeatureManager.Setup(x => x.Get(AppFeatures.DefaultLanguage))
                .Returns(new Feature(AppFeatures.DefaultLanguage, "en"));

            _service = new LanguageService(_context, _mockFeatureManager.Object);
        }

        [Fact]
        public async Task ChangeLanguageAsync_RemovesFeatureSetting_WhenLanguageIsDefault_AndSettingExists()
        {
            _context.FeatureSettings.Add(new MockApi.Models.FeatureSetting
            {
                UserId = _userId,
                Name = AppFeatures.DefaultLanguage,
                Value = "fr"
            });
            await _context.SaveChangesAsync();

            await _service.ChangeLanguageAsync("en", _userId);

            var setting = await _context.FeatureSettings
                .FirstOrDefaultAsync(x => x.UserId == _userId && x.Name == AppFeatures.DefaultLanguage);

            Assert.Null(setting);
        }

        [Fact]
        public async Task ChangeLanguageAsync_UpdatesFeatureSetting_WhenSettingExists()
        {
            _context.FeatureSettings.Add(new MockApi.Models.FeatureSetting
            {
                UserId = _userId,
                Name = AppFeatures.DefaultLanguage,
                Value = "fr"
            });
            await _context.SaveChangesAsync();

            await _service.ChangeLanguageAsync("de", _userId);

            var setting = await _context.FeatureSettings
                .FirstOrDefaultAsync(x => x.UserId == _userId && x.Name == AppFeatures.DefaultLanguage);

            Assert.NotNull(setting);
            Assert.Equal("de", setting!.Value);
        }

        [Fact]
        public async Task ChangeLanguageAsync_CreatesNewFeatureSetting_WhenNotExists()
        {
            await _service.ChangeLanguageAsync("de", _userId);

            var setting = await _context.FeatureSettings
                .FirstOrDefaultAsync(x => x.UserId == _userId && x.Name == AppFeatures.DefaultLanguage);

            Assert.NotNull(setting);
            Assert.Equal("de", setting!.Value);
        }
    }
}
