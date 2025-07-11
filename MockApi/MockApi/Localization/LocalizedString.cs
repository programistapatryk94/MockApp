using System.Globalization;

namespace MockApi.Localization
{
    public class LocalizedString
    {
        public CultureInfo CultureInfo { get; internal set; }
        public string Name { get; private set; }
        public string Value { get; private set; }

        public LocalizedString(string name, string value, CultureInfo cultureInfo)
        {
            Name = name;
            Value = value;
            CultureInfo = cultureInfo;
        }
    }
}
