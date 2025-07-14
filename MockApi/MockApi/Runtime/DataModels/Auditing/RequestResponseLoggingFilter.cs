using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MockApi.Extensions;
using MockApi.Models;
using System.Diagnostics;

namespace MockApi.Runtime.DataModels.Auditing
{
    public class RequestResponseLoggingFilter : IAsyncActionFilter
    {
        private readonly IRequestLogConfiguration _requestLogConfiguration;
        private readonly IRequestLogHelper _requestLogHelper;
        private readonly IAuditSerializer _auditSerializer;

        public RequestResponseLoggingFilter(IRequestLogConfiguration requestLogConfiguration, IRequestLogHelper requestLogHelper, IAuditSerializer auditSerializer)
        {
            _requestLogConfiguration = requestLogConfiguration;
            _requestLogHelper = requestLogHelper;
            _auditSerializer = auditSerializer;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if(!ShouldSaveAudit(context))
            {
                await next();
                return;
            }

            var auditInfo = _requestLogHelper.CreateAuditLog(context.ActionDescriptor.AsControllerActionDescriptor().ControllerTypeInfo.AsType(),
                context.ActionDescriptor.AsControllerActionDescriptor().MethodInfo,
                context.ActionArguments);

            var stopwatch = Stopwatch.StartNew();

            ActionExecutedContext result = null;

            try
            {
                result = await next();
                if (result.Exception != null && !result.ExceptionHandled)
                {
                    auditInfo.Exception = RequestLog.GetAppException(result.Exception);
                }
            }
            catch(Exception ex)
            {
                auditInfo.Exception = RequestLog.GetAppException(ex);
                throw;
            }finally
            {
                stopwatch.Stop();
                auditInfo.ExecutionDuration = Convert.ToInt32(stopwatch.Elapsed.TotalMilliseconds);

                if (_requestLogConfiguration.SaveReturnValues && result != null)
                {
                    switch (result.Result)
                    {
                        case ObjectResult objectResult:
                            auditInfo.ReturnValue = _auditSerializer.Serialize(objectResult.Value);
                            break;

                        case JsonResult jsonResult:
                            auditInfo.ReturnValue = _auditSerializer.Serialize(jsonResult.Value);
                            break;

                        case ContentResult contentResult:
                            auditInfo.ReturnValue = contentResult.Content;
                            break;
                    }
                }

                await _requestLogHelper.SaveAsync(auditInfo);
            }
        }

        private bool ShouldSaveAudit(ActionExecutingContext actionContext)
        {
            return actionContext.ActionDescriptor.IsControllerAction() && _requestLogHelper.ShouldSaveLog(actionContext.ActionDescriptor.GetMethodInfo(), true);
        }
    }
}
