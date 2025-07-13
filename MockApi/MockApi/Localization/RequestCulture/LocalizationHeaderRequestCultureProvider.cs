using Microsoft.AspNetCore.Localization;

namespace MockApi.Localization.RequestCulture
{
    public class LocalizationHeaderRequestCultureProvider : RequestCultureProvider
    {
        private const char _separator = '|';
        private const string _culturePrefix = "c=";
        private const string _uiCulturePrefix = "uic=";
        public string HeaderName { get; set; } = CookieRequestCultureProvider.DefaultCookieName;

        public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            var header = httpContext.Request.Headers[HeaderName];

            if (string.IsNullOrEmpty(header))
            {
                return NullProviderCultureResult;
            }

            var providerResultCulture = CookieRequestCultureProvider.ParseCookieValue(header);

            return Task.FromResult<ProviderCultureResult?>(providerResultCulture);
        }
    }
}
