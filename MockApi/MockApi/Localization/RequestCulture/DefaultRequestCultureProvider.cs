using Microsoft.AspNetCore.Localization;
using MockApi.Helpers;
using MockApi.Runtime.Features;

namespace MockApi.Localization.RequestCulture
{
    public class DefaultRequestCultureProvider : RequestCultureProvider
    {
        public override async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
        {
            var featureChecker = httpContext.RequestServices.GetRequiredService<IFeatureChecker>();
            var culture = await featureChecker.GetValueAsync(AppFeatures.DefaultLanguage);

            if(string.IsNullOrEmpty(culture))
            {
                return null;
            }

            return new ProviderCultureResult(culture, culture);
        }
    }
}
