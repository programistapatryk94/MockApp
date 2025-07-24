using Microsoft.AspNetCore.Mvc.Filters;

namespace MockApi.Runtime.Results
{
    public class NullAppActionResultWrapper : IAppActionResultWrapper
    {
        public void Wrap(FilterContext context)
        {
            
        }
    }
}
