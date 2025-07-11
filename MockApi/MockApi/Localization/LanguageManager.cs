using MockApi.Helpers;
using MockApi.Runtime.Exceptions;
using MockApi.Runtime.Features;
using System.Collections.Immutable;
using System.Globalization;

namespace MockApi.Localization
{
    public class LanguageManager : ILanguageManager
    {
        private readonly ILocalizationConfiguration _configuration;
        private readonly IFeatureChecker _featureChecker;

        public LanguageManager(ILocalizationConfiguration configuration, IFeatureChecker featureChecker)
        {
            _configuration = configuration;
            _featureChecker = featureChecker;
        }

        public LanguageInfo CurrentLanguage { get { return GetCurrentLanguage(); } }

        public IReadOnlyList<LanguageInfo> GetLanguages()
        {
            var defaultLanguageName = GetDefaultLanguageOrNull();

            var list = _configuration.Languages
                .Select(lang => new LanguageInfo(lang.Name, lang.DisplayName, lang.Icon = lang.Icon, lang.Name == defaultLanguageName)).ToImmutableList();

            return list;
        }

        private LanguageInfo GetCurrentLanguage()
        {
            var languages = GetLanguages();
            if (languages.Count <= 0)
            {
                throw new AppException("No language defined in this application.");
            }

            var currentCultureName = CultureInfo.CurrentUICulture.Name;

            var currentLanguage = languages.FirstOrDefault(l => l.Name == currentCultureName);
            if (currentLanguage != null)
            {
                return currentLanguage;
            }

            currentLanguage = languages.FirstOrDefault(l => currentCultureName.StartsWith(l.Name));
            if (currentLanguage != null)
            {
                return currentLanguage;
            }

            currentLanguage = languages.FirstOrDefault(l => l.IsDefault);
            if (currentLanguage != null)
            {
                return currentLanguage;
            }

            return languages[0];
        }

        private string GetDefaultLanguageOrNull()
        {
            var defaultLanguageName = _featureChecker.GetValue(AppFeatures.DefaultLanguage);

            return defaultLanguageName;
        }
    }
}
