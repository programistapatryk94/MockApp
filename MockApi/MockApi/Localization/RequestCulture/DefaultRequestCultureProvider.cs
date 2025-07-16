using Microsoft.AspNetCore.Localization;
using MockApi.Helpers;
using MockApi.Runtime.Features;

namespace MockApi.Localization.RequestCulture
{
    public class DefaultRequestCultureProvider : RequestCultureProvider
    {
        private readonly ILogger<DefaultRequestCultureProvider> _logger;

        public DefaultRequestCultureProvider(ILogger<DefaultRequestCultureProvider> logger)
        {
            _logger = logger;
        }

        public override async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
        {
            var featureChecker = httpContext.RequestServices.GetRequiredService<IFeatureChecker>();
            var culture = await featureChecker.GetValueAsync(AppFeatures.DefaultLanguage);

            if(string.IsNullOrEmpty(culture))
            {
                return null;
            }

            _logger.LogDebug(string.Format("{0} - Using Culture:{1} , UICulture:{2}", nameof(DefaultRequestCultureProvider), culture, culture));

            return new ProviderCultureResult(culture, culture);
        }
    }
}
