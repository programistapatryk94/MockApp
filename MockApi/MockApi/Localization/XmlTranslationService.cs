using MockApi.Runtime.Exceptions;
using System.Globalization;
using System.Xml.Linq;

namespace MockApi.Localization
{
    public class XmlTranslationService : ITranslationService
    {
        public IDictionary<string, ILocalizationDictionary> Dictionaries { get; private set; }

        private readonly ILocalizationConfiguration _localizationConfiguration;
        private readonly ILogger<XmlTranslationService> _logger;

        public XmlTranslationService(ILocalizationConfiguration localizationConfiguration, ILogger<XmlTranslationService> logger)
        {
            _localizationConfiguration = localizationConfiguration;
            Dictionaries = new Dictionary<string, ILocalizationDictionary>();
            _logger = logger;
        }

        public void Initialize()
        {
            _logger.LogDebug(string.Format("Initializing {0} localization sources.", _localizationConfiguration.Sources.Count));

            foreach (var source in _localizationConfiguration.Sources)
            {
                var basePath = Path.Combine(AppContext.BaseDirectory, source.RelativePathToFolder);

                if (!Directory.Exists(basePath))
                    throw new AppException($"Translation directory not found: {basePath}");

                var xmlFiles = Directory.GetFiles(basePath, "*.xml", SearchOption.TopDirectoryOnly);

                foreach (var filePath in xmlFiles)
                {
                    var xmlString = File.ReadAllText(filePath);

                    var xdoc = XDocument.Parse(xmlString);

                    var root = xdoc.Element("translations");
                    if (root == null)
                        throw new AppException("A translation XML must include translations as root node.");

                    var langCode = root.Attribute("culture")?.Value?.ToLower();
                    if (string.IsNullOrWhiteSpace(langCode))
                        throw new AppException($"Missing required 'culture' attribute in resource: {Path.GetFileName(filePath)}");

                    var culture = CultureInfo.GetCultureInfo(langCode);
                    if (!Dictionaries.ContainsKey(culture.Name))
                    {
                        Dictionaries[culture.Name] = new LocalizationDictionary(culture);
                    }

                    var entries = root.Elements("text")
                    .Where(x => x.Attribute("key") != null)
                    .ToDictionary(
                        x => x.Attribute("key")!.Value,
                        x => x.Value.Trim()
                    );

                    foreach (var (key, value) in entries)
                    {
                        if (Dictionaries[culture.Name].ContainsKey(key))
                        {
                            throw new AppException($"Duplicate translation key '{key}' found for language '{langCode}' in resource '{Path.GetFileName(filePath)}'.");
                        }

                        Dictionaries[culture.Name][key] = value;
                    }
                }

                _logger.LogDebug("Initialized localization source: " + source.RelativePathToFolder);
            }
        }

        public string Translate(string key, CultureInfo culture)
        {
            if (Dictionaries.TryGetValue(culture.Name, out var dict) && dict.ContainsKey(key))
            {
                return dict[key];
            }

            if (!culture.IsNeutralCulture)
            {
                var parentCulture = culture.Parent;
                if (Dictionaries.TryGetValue(parentCulture.Name, out var parentDict) && parentDict.ContainsKey(key))
                {
                    return parentDict[key];
                }
            }

            if (Dictionaries.TryGetValue("en", out var fallbackDict) && fallbackDict.ContainsKey(key))
            {
                return fallbackDict[key];
            }

            return key;
        }

        public string Translate(string key)
        {
            return Translate(key, CultureInfo.CurrentUICulture);
        }

        public string Translate(string key, params object[] args)
        {
            return string.Format(Translate(key), args);
        }
    }
}
