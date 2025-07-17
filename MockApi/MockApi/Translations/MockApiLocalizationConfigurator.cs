using Microsoft.AspNetCore.Localization;
using MockApi.Localization;
using MockApi.Localization.RequestCulture;
using MockApi.Runtime.Dependency;
using System.Globalization;

namespace MockApi.Translations
{
    public static class MockApiLocalizationConfigurator
    {
        public static void Configure(ILocalizationConfiguration localizationConfiguration)
        {
            localizationConfiguration.Languages.Add(new LanguageInfo("en", "English", "flag-icon us", isDefault: true));
            localizationConfiguration.Languages.Add(new LanguageInfo("pl", "Polski", "flag-icon pl"));

            localizationConfiguration.Sources.Add(new LocalizationSource(Path.Combine("Translations", "SourceFiles")));
        }

        public static void UseAppRequestLocalization(this IApplicationBuilder app, Action<RequestLocalizationOptions> optionsAction = null)
        {
            var serviceProvider = app.ApplicationServices;

            using (var languageManager = serviceProvider.ResolveAsDisposable<ILanguageManager>())
            {
                var supportedCultures = languageManager.Object.GetLanguages()
                    .Select(l => CultureInfo.GetCultureInfo(l.Name))
                    .ToArray();

                var options = new RequestLocalizationOptions
                {
                    SupportedCultures = supportedCultures,
                    SupportedUICultures = supportedCultures
                };

                var userProvider = serviceProvider.GetRequiredService<UserRequestCultureProvider>();
                var headerProvider = serviceProvider.GetRequiredService<LocalizationHeaderRequestCultureProvider>();
                var defaultProvider = serviceProvider.GetRequiredService<DefaultRequestCultureProvider>();

                options.RequestCultureProviders.Insert(1, userProvider);
                options.RequestCultureProviders.Insert(2, headerProvider);
                options.RequestCultureProviders.Insert(5, defaultProvider);

                optionsAction?.Invoke(options);

                userProvider.CookieProvider = options.RequestCultureProviders.OfType<CookieRequestCultureProvider>().FirstOrDefault();
                userProvider.HeaderProvider = options.RequestCultureProviders.OfType<LocalizationHeaderRequestCultureProvider>().FirstOrDefault();

                app.UseRequestLocalization(options);
            }
        }
    }
}
