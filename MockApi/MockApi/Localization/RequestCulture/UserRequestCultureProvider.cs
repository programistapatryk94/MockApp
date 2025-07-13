using Microsoft.AspNetCore.Localization;
using MockApi.Data;
using MockApi.Helpers;
using MockApi.Runtime.Features;
using MockApi.Runtime.Session;
using System.Diagnostics.CodeAnalysis;

namespace MockApi.Localization.RequestCulture
{
    public class UserRequestCultureProvider : RequestCultureProvider
    {
        public CookieRequestCultureProvider CookieProvider { get; set; }
        public LocalizationHeaderRequestCultureProvider HeaderProvider { get; set; }

        public override async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
        {
            var appSession = httpContext.RequestServices.GetRequiredService<IAppSession>();
            if (!appSession.UserId.HasValue)
            {
                return null;
            }

            var featureChecker = httpContext.RequestServices.GetRequiredService<IFeatureChecker>();

            var userCulture = await featureChecker.GetValueAsync(AppFeatures.DefaultLanguage, false);

            if (!string.IsNullOrEmpty(userCulture))
            {
                return new ProviderCultureResult(userCulture, userCulture);
            }

            ProviderCultureResult? result = null;
            string? cultureName = null;
            var cookieResult = await GetResultOrNull(httpContext, CookieProvider);
            if (cookieResult != null && cookieResult.Cultures.Any())
            {
                var cookieCulture = cookieResult.Cultures.First().Value;
                var cookieUICulture = cookieResult.UICultures.First().Value;

                result = cookieResult;
                cultureName = cookieCulture ?? cookieUICulture;
            }

            if (result == null || !result.Cultures.Any())
            {
                var headerResult = await GetResultOrNull(httpContext, HeaderProvider);
                if (headerResult != null && headerResult.Cultures.Any())
                {
                    var headerCulture = headerResult.Cultures.First().Value;
                    var headerUICulture = headerResult.UICultures.First().Value;

                    result = headerResult;
                    cultureName = headerCulture ?? headerUICulture;
                }
            }

            if (string.IsNullOrEmpty(cultureName) || cultureName == await featureChecker.GetValueAsync(AppFeatures.DefaultLanguage))
            {
                return result;
            }

            var languageManager = httpContext.RequestServices.GetRequiredService<ILanguageManager>();
            var languageService = httpContext.RequestServices.GetRequiredService<ILanguageService>();

            var languages = languageManager.GetLanguages();
            if (languages.Any(p => p.Name.ToLowerInvariant() == cultureName.ToLowerInvariant()))
            {
                await languageService.ChangeLanguageAsync(cultureName, appSession.UserId.Value);
            }

            return result;
        }

        protected virtual async Task<ProviderCultureResult?> GetResultOrNull([NotNull] HttpContext httpContext, IRequestCultureProvider? provider)
        {
            if (provider == null)
            {
                return null;
            }

            return await provider.DetermineProviderCultureResult(httpContext);
        }
    }
}
