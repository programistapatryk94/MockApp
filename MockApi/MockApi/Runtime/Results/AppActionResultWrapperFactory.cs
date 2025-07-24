using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MockApi.Runtime.Results
{
    public class AppActionResultWrapperFactory : IAppActionResultWrapperFactory
    {
        public IAppActionResultWrapper CreateFor(FilterContext context)
        {
            if(null == context) {
                throw new ArgumentNullException(nameof(context));
            }

            switch(context)
            {
                case ResultExecutingContext resultExecutingContext when resultExecutingContext.Result is ObjectResult:
                    return new AppObjectActionResultWrapper();
                case ResultExecutingContext resultExecutingContext when resultExecutingContext.Result is JsonResult:
                    return new AppJsonActionResultWrapper();
                case ResultExecutingContext resultExecutingContext when resultExecutingContext.Result is EmptyResult:
                    return new AppEmptyActionResultWrapper();
                default:
                    return new NullAppActionResultWrapper();
            }
        }
    }
}
