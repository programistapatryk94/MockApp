namespace MockApi.Localization
{
    public interface ILanguageService
    {
        Task ChangeLanguageAsync(string languageName, Guid userId);
    }
}
