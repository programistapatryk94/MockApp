using Microsoft.AspNetCore.Mvc.Filters;

namespace MockApi.Runtime.Results
{
    public interface IAppActionResultWrapperFactory
    {
        IAppActionResultWrapper CreateFor(FilterContext context);
    }
}
