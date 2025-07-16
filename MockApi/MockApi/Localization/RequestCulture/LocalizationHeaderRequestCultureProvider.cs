using Microsoft.AspNetCore.Localization;

namespace MockApi.Localization.RequestCulture
{
    public class LocalizationHeaderRequestCultureProvider : RequestCultureProvider
    {
        public string HeaderName { get; set; } = CookieRequestCultureProvider.DefaultCookieName;
        private readonly ILogger<LocalizationHeaderRequestCultureProvider> _logger;

        public LocalizationHeaderRequestCultureProvider(ILogger<LocalizationHeaderRequestCultureProvider> logger)
        {
            _logger = logger;
        }

        public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            var header = httpContext.Request.Headers[HeaderName];

            if (string.IsNullOrEmpty(header))
            {
                return NullProviderCultureResult;
            }

            var providerResultCulture = CookieRequestCultureProvider.ParseCookieValue(header);

            _logger.LogDebug(string.Format("{0} - Using Culture:{1} , UICulture:{2}", nameof(LocalizationHeaderRequestCultureProvider), providerResultCulture?.Cultures.FirstOrDefault(), providerResultCulture?.UICultures.FirstOrDefault()));

            return Task.FromResult<ProviderCultureResult?>(providerResultCulture);
        }
    }
}
