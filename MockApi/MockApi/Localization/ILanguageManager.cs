namespace MockApi.Localization
{
    public interface ILanguageManager
    {
        LanguageInfo CurrentLanguage { get; }
        IReadOnlyList<LanguageInfo> GetLanguages();
    }
}
