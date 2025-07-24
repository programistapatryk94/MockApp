using Microsoft.AspNetCore.Mvc.Filters;

namespace MockApi.Runtime.Results
{
    public interface IAppActionResultWrapper
    {
        void Wrap(FilterContext context);
    }
}
