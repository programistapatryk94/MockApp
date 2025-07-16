using MockApi.Runtime.Exceptions.Handling;

namespace MockApi.Runtime.Configuration
{
    public interface IAppConfiguration
    {
        WrapExceptionAttribute DefaultWrapExceptionAttribute { get; }
    }
}
