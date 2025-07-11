using MockApi.Localization;

namespace MockApi.Dtos.Auth
{
    public class LocalizationConfigurationDto
    {
        public CurrentCultureConfigDto CurrentCulture { get; set; }
        public List<LanguageInfo> Languages { get; set; }
    }
}
