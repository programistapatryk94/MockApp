using Microsoft.AspNetCore.Mvc.Filters;
using MockApi.Extensions;
using MockApi.Helpers;
using MockApi.Runtime.Configuration;

namespace MockApi.Runtime.Results
{
    public class AppResultFilter : IResultFilter
    {
        private readonly IAppConfiguration _appConfiguration;
        private readonly IAppActionResultWrapperFactory _appActionResultWrapperFactory;

        public AppResultFilter(IAppConfiguration appConfiguration, IAppActionResultWrapperFactory appActionResultWrapperFactory)
        {
            _appConfiguration = appConfiguration;
            _appActionResultWrapperFactory = appActionResultWrapperFactory;
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            //empty
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (!context.ActionDescriptor.IsControllerAction())
            {
                return;
            }

            var methodInfo = context.ActionDescriptor.GetMethodInfo();

            var wrapResultAttribute =
                ReflectionHelper.GetSingleAttributeOfMemberOrDeclaringTypeOrDefault(
                    methodInfo,
                    _appConfiguration.DefaultWrapResultAttribute
                );

            if (!wrapResultAttribute.WrapOnSuccess)
            {
                return;
            }

            _appActionResultWrapperFactory.CreateFor(context).Wrap(context);
        }
    }
}
