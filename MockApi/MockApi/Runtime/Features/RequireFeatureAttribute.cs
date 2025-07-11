using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MockApi.Runtime.Session;

namespace MockApi.Runtime.Features
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireFeatureAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _featureName;

        public RequireFeatureAttribute(string featureName)
        {
            _featureName = featureName;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var featureChecker = context.HttpContext.RequestServices.GetService<IFeatureChecker>();
            var session = context.HttpContext.RequestServices.GetService<IAppSession>();

            if (featureChecker == null || session == null || session.UserId == null)
            {
                context.Result = new ForbidResult(); // lub UnauthorizedResult()
                return;
            }

            var isEnabled = await featureChecker.IsEnabledAsync(_featureName);

            if (!isEnabled)
            {
                context.Result = new ForbidResult(); // blokujemy dostęp
            }
        }
    }
}
