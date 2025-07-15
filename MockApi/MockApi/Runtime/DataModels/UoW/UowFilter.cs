using Microsoft.AspNetCore.Mvc.Filters;
using MockApi.Extensions;
using System.Transactions;

namespace MockApi.Runtime.DataModels.UoW
{
    public class UowFilter : IAsyncActionFilter
    {
        private readonly IUnitOfWorkConfiguration _unitOfWorkConfiguration;

        public UowFilter(IUnitOfWorkConfiguration unitOfWorkConfiguration)
        {
            _unitOfWorkConfiguration = unitOfWorkConfiguration;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionDescriptor.IsControllerAction())
            {
                await next();
                return;
            }

            var unitOfWorkAttr = _unitOfWorkConfiguration.GetUnitOfWorkAttributeOrNull(context.ActionDescriptor.GetMethodInfo());

            if (null == unitOfWorkAttr)
            {
                await next();
                return;
            }

            var options = new TransactionOptions
            {
                IsolationLevel = unitOfWorkAttr.IsolationLevel ?? _unitOfWorkConfiguration.IsolationLevel ?? IsolationLevel.ReadCommitted,
                Timeout = unitOfWorkAttr.Timeout ?? _unitOfWorkConfiguration.Timeout ?? TransactionManager.DefaultTimeout
            };

            using var scope = new TransactionScope(
                unitOfWorkAttr.Scope ?? _unitOfWorkConfiguration.Scope,
                options,
                TransactionScopeAsyncFlowOption.Enabled
                );

            try
            {
                var resultContext = await next();

                if (resultContext.Exception == null || resultContext.ExceptionHandled)
                {
                    scope.Complete();
                }
                else
                {
                    //rollback
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
