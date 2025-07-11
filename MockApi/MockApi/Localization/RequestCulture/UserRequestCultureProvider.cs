using Microsoft.AspNetCore.Localization;
using MockApi.Helpers;
using MockApi.Runtime.Features;
using MockApi.Runtime.Session;
using System.Diagnostics.CodeAnalysis;

namespace MockApi.Localization.RequestCulture
{
    public class UserRequestCultureProvider : RequestCultureProvider
    {
        public CookieRequestCultureProvider CookieProvider { get; set; }

        public override async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
        {
            var abpSession = httpContext.RequestServices.GetRequiredService<IAppSession>();
            if (!abpSession.UserId.HasValue)
            {
                return null;
            }

            var featureChecker = httpContext.RequestServices.GetRequiredService<IFeatureChecker>();

            var userCulture = await featureChecker.GetValueAsync(AppFeatures.DefaultLanguage);

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
