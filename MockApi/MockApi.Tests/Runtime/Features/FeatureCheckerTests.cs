using Microsoft.EntityFrameworkCore;
using MockApi.Data;
using MockApi.Models;
using MockApi.Runtime.Features;
using MockApi.Runtime.Session;
using Moq;

namespace MockApi.Tests.Runtime.Features
{
    public class FeatureCheckerTests
    {
        private readonly AppDbContext _context;
        private readonly Mock<IAppSession> _mockSession = new();
        private readonly Mock<IFeatureManager> _mockFeatureManager = new();
        private readonly FeatureChecker _checker;
        private readonly Guid _userId = Guid.NewGuid();

        public FeatureCheckerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options, _mockSession.Object);
            _mockSession.Setup(x => x.UserId).Returns(_userId);

            _mockFeatureManager.Setup(x => x.Get("TestFeature")).Returns(new Feature("TestFeature", "default"));

            _checker = new FeatureChecker(_mockSession.Object, _mockFeatureManager.Object, _context);
        }

        [Fact]
        public void GetValue_ReturnsDefault_WhenNoDbValue()
        {
            var value = _checker.GetValue("TestFeature");

            Assert.Equal("default", value);
        }

        [Fact]
        public async Task GetValueAsync_ReturnsDbValue_WhenExists()
        {
            _context.FeatureSettings.Add(new FeatureSetting
            {
                UserId = _userId,
                Name = "TestFeature",
                Value = "custom"
            });
            await _context.SaveChangesAsync();

            var value = await _checker.GetValueAsync("TestFeature");

            Assert.Equal("custom", value);
        }

        [Fact]
        public async Task GetValueAsync_ReturnsDefault_WhenDbValueMissing()
        {
            var value = await _checker.GetValueAsync("TestFeature");

            Assert.Equal("default", value);
        }

        [Fact]
        public async Task GetValueAsync_RespectsFallbackFlag()
        {
            var value = await _checker.GetValueAsync("TestFeature", fallbackToDefault: false);

            Assert.Null(value);
        }

        [Fact]
        public async Task IsEnabledAsync_ReturnsTrue_WhenDbValueIsTrue()
        {
            _context.FeatureSettings.Add(new FeatureSetting
            {
                UserId = _userId,
                Name = "TestFeature",
                Value = "true"
            });
            await _context.SaveChangesAsync();

            var result = await _checker.IsEnabledAsync("TestFeature");

            Assert.True(result);
        }

        [Fact]
        public async Task IsEnabledAsync_ReturnsFalse_WhenDbValueIsFalse()
        {
            _context.FeatureSettings.Add(new FeatureSetting
            {
                UserId = _userId,
                Name = "TestFeature",
                Value = "false"
            });
            await _context.SaveChangesAsync();

            var result = await _checker.IsEnabledAsync("TestFeature");

            Assert.False(result);
        }

        [Fact]
        public async Task GetValueAsync_ByUserId_ReturnsDbValue_WhenExists()
        {
            var anotherUserId = Guid.NewGuid();

            _context.FeatureSettings.Add(new FeatureSetting
            {
                UserId = anotherUserId,
                Name = "TestFeature",
                Value = "custom-for-other"
            });
            await _context.SaveChangesAsync();

            var value = await _checker.GetValueAsync(anotherUserId, "TestFeature");

            Assert.Equal("custom-for-other", value);
        }

        [Fact]
        public async Task GetValueAsync_ByUserId_ReturnsDefault_WhenNotExists()
        {
            var unknownUserId = Guid.NewGuid();

            var value = await _checker.GetValueAsync(unknownUserId, "TestFeature");

            Assert.Equal("default", value);
        }

        [Fact]
        public async Task GetValueAsync_ByUserId_RespectsFallbackFlag()
        {
            var unknownUserId = Guid.NewGuid();

            var value = await _checker.GetValueAsync(unknownUserId, "TestFeature", fallbackToDefault: false);

            Assert.Null(value);
        }

        [Fact]
        public async Task IsEnabledAsync_ByUserId_ReturnsTrue_WhenDbValueIsTrue()
        {
            var userId = Guid.NewGuid();

            _context.FeatureSettings.Add(new FeatureSetting
            {
                UserId = userId,
                Name = "TestFeature",
                Value = "true"
            });
            await _context.SaveChangesAsync();

            var result = await _checker.IsEnabledAsync(userId, "TestFeature");

            Assert.True(result);
        }

        [Fact]
        public async Task IsEnabledAsync_ByUserId_ReturnsFalse_WhenDbValueIsFalse()
        {
            var userId = Guid.NewGuid();

            _context.FeatureSettings.Add(new FeatureSetting
            {
                UserId = userId,
                Name = "TestFeature",
                Value = "false"
            });
            await _context.SaveChangesAsync();

            var result = await _checker.IsEnabledAsync(userId, "TestFeature");

            Assert.False(result);
        }

    }
}
