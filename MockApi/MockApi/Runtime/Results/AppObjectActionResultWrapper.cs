using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MockApi.Runtime.Results
{
    public class AppObjectActionResultWrapper : IAppActionResultWrapper
    {
        public void Wrap(FilterContext context)
        {
            ObjectResult objectResult = null;

            switch(context)
            {
                case ResultExecutingContext resultExecutingContext:
                    objectResult = resultExecutingContext.Result as ObjectResult;
                    break;
            }

            if(objectResult == null)
            {
                throw new ArgumentException("Action Result should be JsonResult!");
            }
        }
    }
}
