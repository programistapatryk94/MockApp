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

                var userProvider = new UserRequestCultureProvider();

                options.RequestCultureProviders.Insert(1, userProvider);
                options.RequestCultureProviders.Insert(2, new LocalizationHeaderRequestCultureProvider());
                options.RequestCultureProviders.Insert(5, new DefaultRequestCultureProvider());

                optionsAction?.Invoke(options);

                userProvider.CookieProvider = options.RequestCultureProviders.OfType<CookieRequestCultureProvider>().FirstOrDefault();
                userProvider.HeaderProvider = options.RequestCultureProviders.OfType<LocalizationHeaderRequestCultureProvider>().FirstOrDefault();

                app.UseRequestLocalization(options);
            }
        }
    }
}
