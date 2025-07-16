using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MockApi.Extensions;
using MockApi.Helpers;
using MockApi.Runtime.Configuration;
using System.Net;

namespace MockApi.Runtime.Exceptions.Handling
{
    public class AppExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<AppExceptionFilter> _logger;
        private readonly IAppConfiguration _appConfiguration;

        public AppExceptionFilter(ILogger<AppExceptionFilter> logger, IAppConfiguration appConfiguration)
        {
            _logger = logger;
            _appConfiguration = appConfiguration;
        }

        public void OnException(ExceptionContext context)
        {
            if(!context.ActionDescriptor.IsControllerAction())
            {
                return;
            }

            var wrapExceptionAttribute = ReflectionHelper.GetSingleAttributeOfMemberOrDeclaringTypeOrDefault(context.ActionDescriptor.GetMethodInfo(), _appConfiguration.DefaultWrapExceptionAttribute);

            if(wrapExceptionAttribute.LogError)
            {
                _logger.Log(LogLevel.Error, context.Exception, context.Exception.Message);
            }

            HandleAndWrapException(context, wrapExceptionAttribute);
        }

        private void HandleAndWrapException(ExceptionContext context, WrapExceptionAttribute wrapExceptionAttribute)
        {
            if(!context.ActionDescriptor.IsReturnTypeObjectResult())
            {
                return;
            }
            var displayUrl = context.HttpContext.Request.GetDisplayUrl();

            context.HttpContext.Response.StatusCode = GetStatusCode(context, wrapExceptionAttribute.WrapOnError);

            if (!wrapExceptionAttribute.WrapOnError)
            {
                return;
            }

            HandleError(context);
        }

        private void HandleError(ExceptionContext context)
        {
            context.Result = new ObjectResult(context.Result);

            context.Exception = null;
        }

        private int GetStatusCode(ExceptionContext context, bool wrapOnError)
        {
            if(wrapOnError)
            {
                return (int)HttpStatusCode.InternalServerError;
            }

            return context.HttpContext.Response.StatusCode;
        }
    }
}
