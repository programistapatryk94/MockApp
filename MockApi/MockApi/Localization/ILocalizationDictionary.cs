using System.Globalization;

namespace MockApi.Localization
{
    public interface ILocalizationDictionary
    {
        CultureInfo CultureInfo { get; }
        string this[string name] { get; set; }
        LocalizedString GetOrNull(string name);
        bool ContainsKey(string name);
    }
}
