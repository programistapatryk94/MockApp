
namespace MockApi.Localization
{
    public class LocalizationConfiguration : ILocalizationConfiguration
    {
        public IList<LanguageInfo> Languages { get; }
        public IList<LocalizationSource> Sources { get; }

        public LocalizationConfiguration()
        {
            Languages = new List<LanguageInfo>();
            Sources = new List<LocalizationSource>();
        }
    }
}
