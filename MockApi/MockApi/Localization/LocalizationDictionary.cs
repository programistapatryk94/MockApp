using System.Globalization;

namespace MockApi.Localization
{
    public class LocalizationDictionary : ILocalizationDictionary
    {
        private readonly Dictionary<string, LocalizedString> _dictionary;
        public CultureInfo CultureInfo { get; private set; }

        public LocalizationDictionary(CultureInfo cultureInfo)
        {
            CultureInfo = cultureInfo;
            _dictionary = new Dictionary<string, LocalizedString>();
        }

        public virtual string this[string name]
        {
            get
            {
                var localizedString = GetOrNull(name);
                return localizedString?.Value;
            }
            set => _dictionary[name] = new LocalizedString(name, value, CultureInfo);
        }

        public virtual LocalizedString GetOrNull(string name)
        {
            return _dictionary.TryGetValue(name, out var localizedString) ? localizedString : null;
        }

        public bool ContainsKey(string name)
        {
            return _dictionary.ContainsKey(name);
        }
    }
}
