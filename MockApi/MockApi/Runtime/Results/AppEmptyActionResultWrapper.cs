using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MockApi.Runtime.Results
{
    public class AppEmptyActionResultWrapper : IAppActionResultWrapper
    {
        public void Wrap(FilterContext context)
        {
            switch (context)
            {
                case ResultExecutingContext resultExecutingContext:
                    //resultExecutingContext.Result = new ObjectResult();
                    return;
            }
        }
    }
}
