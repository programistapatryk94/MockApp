namespace MockApi.Localization
{
    public interface ILocalizationConfiguration
    {
        IList<LanguageInfo> Languages { get; }
        IList<LocalizationSource> Sources { get; }
    }
}
