using System.Globalization;

namespace MockApi.Localization
{
    public interface ITranslationService
    {
        string Translate(string key);
        string Translate(string key, CultureInfo culture);
        string Translate(string key, params object[] args);
    }
}
