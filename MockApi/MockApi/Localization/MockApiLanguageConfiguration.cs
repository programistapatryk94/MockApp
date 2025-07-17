namespace MockApi.Localization
{
    public static class MockApiLanguageConfiguration
    {
        public static void Configure(ILocalizationConfiguration localizationConfiguration)
        {
            localizationConfiguration.Languages.Add(new LanguageInfo("en", "English", "flag-icon us", isDefault: true));
            localizationConfiguration.Languages.Add(new LanguageInfo("pl", "Polski", "flag-icon pl"));

            localizationConfiguration.Sources.Add(new LocalizationSource(Path.Combine("Translations", "SourceFiles")));
        }
    }
}
