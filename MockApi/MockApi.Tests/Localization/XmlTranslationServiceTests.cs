using Microsoft.Extensions.Logging;
using MockApi.Localization;
using MockApi.Runtime.Exceptions;
using Moq;
using System.Globalization;

namespace MockApi.Tests.Localization
{
    public class XmlTranslationServiceTests : IDisposable
    {
        private readonly string _testDir;
        private readonly Mock<ILogger<XmlTranslationService>> _mockLogger = new();
        private readonly Mock<ILocalizationConfiguration> _mockConfig;
        private readonly XmlTranslationService _service;

        public XmlTranslationServiceTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDir);

            _mockConfig = new Mock<ILocalizationConfiguration>();
            _mockConfig.Setup(c => c.Sources).Returns(new List<LocalizationSource>
            {
                new LocalizationSource(_testDir)
            });

            _service = new XmlTranslationService(_mockConfig.Object, _mockLogger.Object);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, true);
        }

        private void WriteFile(string fileName, string content)
        {
            File.WriteAllText(Path.Combine(_testDir, fileName), content);
        }

        [Fact]
        public void Initialize_LoadsTranslationsCorrectly()
        {
            // Arrange
            var xml = @"<translations culture='en'>
                            <text key='Hello'>Hello</text>
                            <text key='Bye'>Goodbye</text>
                        </translations>";
            WriteFile("en.xml", xml);

            // Act
            _service.Initialize();

            // Assert
            Assert.True(_service.Dictionaries.ContainsKey("en"));
            var dict = _service.Dictionaries["en"];
            Assert.Equal("Hello", dict["Hello"]);
            Assert.Equal("Goodbye", dict["Bye"]);
        }

        [Fact]
        public void Initialize_ThrowsException_WhenDirectoryNotFound()
        {
            // Arrange
            var badPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _mockConfig.Setup(c => c.Sources).Returns(new List<LocalizationSource>
            {
                new LocalizationSource(badPath)
            });

            var service = new XmlTranslationService(_mockConfig.Object, _mockLogger.Object);

            // Act & Assert
            var ex = Assert.Throws<AppException>(() => service.Initialize());
            Assert.Contains("Translation directory not found", ex.Message);
        }

        [Fact]
        public void Initialize_ThrowsException_WhenRootMissing()
        {
            WriteFile("missing_root.xml", "<invalidRoot culture='en'></invalidRoot>");

            var ex = Assert.Throws<AppException>(() => _service.Initialize());
            Assert.Contains("include translations as root node", ex.Message);
        }

        [Fact]
        public void Initialize_ThrowsException_WhenCultureMissing()
        {
            WriteFile("missing_culture.xml", "<translations><text key='x'>x</text></translations>");

            var ex = Assert.Throws<AppException>(() => _service.Initialize());
            Assert.Contains("Missing required 'culture' attribute", ex.Message);
        }

        [Fact]
        public void Initialize_ThrowsException_WhenDuplicateKeyExists()
        {
            var xml = @"<translations culture='en'>
                            <text key='key1'>value1</text>
                            <text key='key1'>value2</text>
                        </translations>";
            WriteFile("dup.xml", xml);

            var ex = Assert.Throws<AppException>(() => _service.Initialize());
            Assert.Contains("Duplicate translation key", ex.Message);
        }

        [Fact]
        public void Translate_UsesCurrentUICulture_WhenNoCultureProvided()
        {
            // Arrange
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("pl");

            WriteFile("texts.pl.xml", @"<translations culture='pl'>
                            <text key='greeting'>Cześć</text>
                        </translations>");
            _service.Initialize();

            // Act
            var result = _service.Translate("greeting");

            // Assert
            Assert.Equal("Cześć", result);
        }

        [Fact]
        public void Translate_FallsBackToDefaultCulture_WhenCurrentUICultureMissingKey()
        {
            // Arrange
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr");

            WriteFile("texts.en.xml", @"<translations culture='en'>
            <text key='greeting'>Hello</text>
        </translations>");

            WriteFile("texts.fr.xml", @"<translations culture='fr'>
        </translations>");
            _service.Initialize();

            // Act
            var result = _service.Translate("greeting");

            // Assert
            Assert.Equal("Hello", result); // fallback to English
        }

        [Fact]
        public void Translate_ReturnsKey_WhenKeyNotFoundInAnyCulture()
        {
            // Arrange
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("es");

            WriteFile("texts.es.xml", @"<translations culture='es'>
        </translations>");
            _service.Initialize();

            // Act
            var result = _service.Translate("nonexistent_key");

            // Assert
            Assert.Equal("nonexistent_key", result);
        }

    }
}
